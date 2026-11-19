using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using NetSniffer.Core.Details;
using NetSniffer.Core.Filtering;
using NetSniffer.Core.Flows;
using NetSniffer.Core.Models;
using NetSniffer.Core.Parsing;
using NetSniffer.Core.Services;

namespace NetSniffer.App;

public partial class Form1 : Form
{
    private const int MaximumVisiblePackets = 2_000;
    private const int MaximumPendingPackets = 1_000;
    private const int MaximumPacketsPerRefresh = 250;
    private const int MaximumRetainedPackets = 5_000;

    private readonly ICaptureDeviceCatalog _deviceCatalog;
    private readonly IPacketCaptureService _captureService;
    private readonly IPacketParser _packetParser;
    private readonly IPacketDetailParser _packetDetailParser;
    private readonly ConcurrentQueue<PacketDisplayItem> _pendingPackets = new();
    private readonly List<PacketDisplayItem> _capturedPackets = [];
    private PacketFilter _activeFilter = PacketFilter.All;
    private TcpFlowKey? _trackedTcpFlow;
    private long _capturedPacketCount;
    private long _droppedDisplayPacketCount;
    private int _acceptPackets;

    public Form1()
        : this(
            new SharpPcapDeviceCatalog(),
            new SharpPcapCaptureService(),
            new PacketParser(),
            new PacketDetailParser())
    {
    }

    internal Form1(
        ICaptureDeviceCatalog deviceCatalog,
        IPacketCaptureService captureService,
        IPacketParser packetParser,
        IPacketDetailParser packetDetailParser)
    {
        _deviceCatalog = deviceCatalog;
        _captureService = captureService;
        _packetParser = packetParser;
        _packetDetailParser = packetDetailParser;

        InitializeComponent();
        InitializeFilterControls();

        Shown += HandleShown;
        FormClosing += HandleFormClosing;
        refreshButton.Click += HandleRefreshClick;
        startButton.Click += HandleStartClick;
        stopButton.Click += HandleStopClick;
        clearButton.Click += HandleClearClick;
        followTcpStreamButton.Click += HandleFollowTcpStreamClick;
        clearTcpStreamButton.Click += HandleClearTcpStreamClick;
        applyFilterButton.Click += HandleApplyFilterClick;
        resetFilterButton.Click += HandleResetFilterClick;
        packetListView.SelectedIndexChanged += HandlePacketSelectionChanged;
        uiTimer.Tick += HandleUiTimerTick;

        _captureService.PacketCaptured += HandlePacketCaptured;
        _captureService.CaptureStopped += HandleCaptureStopped;
        _captureService.CaptureFailed += HandleCaptureFailed;
    }

    private async void HandleShown(object? sender, EventArgs e)
    {
        ApplyInitialSplitterPositions();
        await RefreshDevicesAsync();
    }

    private async void HandleRefreshClick(object? sender, EventArgs e)
    {
        await RefreshDevicesAsync();
    }

    private void HandleStartClick(object? sender, EventArgs e)
    {
        if (deviceComboBox.SelectedItem is not CaptureDeviceInfo selectedDevice)
        {
            MessageBox.Show(
                this,
                "请先选择一个可用网卡。",
                "未选择网卡",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            ClearPacketDisplay();
            Interlocked.Exchange(ref _acceptPackets, 1);
            uiTimer.Start();
            _captureService.Start(selectedDevice.Name);
            SetCaptureState(true);
            statusTextLabel.Text = $"正在抓包：{selectedDevice.Description}";
        }
        catch (Exception exception)
        {
            Interlocked.Exchange(ref _acceptPackets, 0);
            uiTimer.Stop();
            SetCaptureState(false);
            ShowCaptureError("无法开始抓包", exception);
        }
    }

    private async void HandleStopClick(object? sender, EventArgs e)
    {
        await StopCaptureAsync(showErrorDialog: true);
    }

    private void HandleClearClick(object? sender, EventArgs e)
    {
        ClearPacketDisplay();
    }

    private void HandleApplyFilterClick(object? sender, EventArgs e)
    {
        ApplyFilterFromControls();
    }

    private void HandleResetFilterClick(object? sender, EventArgs e)
    {
        protocolFilterComboBox.SelectedIndex = 0;
        ipFilterTextBox.Clear();
        portFilterTextBox.Clear();
        _activeFilter = PacketFilter.All;
        ApplyFilterToVisiblePackets();
    }

    private void HandlePacketSelectionChanged(object? sender, EventArgs e)
    {
        DisplaySelectedPacket();
        UpdateTcpStreamButtons();
    }

    private void HandleFollowTcpStreamClick(object? sender, EventArgs e)
    {
        if (!TryGetSelectedPacket(out var selectedPacket) ||
            !TcpFlowKey.TryCreate(selectedPacket.Packet, out var flowKey))
        {
            return;
        }

        _trackedTcpFlow = flowKey;
        ApplyFilterToVisiblePackets();
        statusTextLabel.Text = $"正在追踪 TCP 流：{flowKey.DisplayName}";
    }

    private void HandleClearTcpStreamClick(object? sender, EventArgs e)
    {
        _trackedTcpFlow = null;
        ApplyFilterToVisiblePackets();
        statusTextLabel.Text = "已退出 TCP 流追踪";
    }

    private void HandlePacketCaptured(object? sender, PacketCapturedEventArgs e)
    {
        if (Volatile.Read(ref _acceptPackets) == 0)
        {
            return;
        }

        var parsedPacket = _packetParser.Parse(e.Packet);

        if (Volatile.Read(ref _acceptPackets) == 0)
        {
            return;
        }

        var sequenceNumber = Interlocked.Increment(ref _capturedPacketCount);
        _pendingPackets.Enqueue(new PacketDisplayItem(sequenceNumber, parsedPacket));

        while (_pendingPackets.Count > MaximumPendingPackets && _pendingPackets.TryDequeue(out _))
        {
            Interlocked.Increment(ref _droppedDisplayPacketCount);
        }
    }

    private void HandleCaptureStopped(object? sender, CaptureStoppedEventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        BeginInvoke(() =>
        {
            Interlocked.Exchange(ref _acceptPackets, 0);
            uiTimer.Stop();

            try
            {
                // Also releases the device when SharpPcap stops because of a capture error.
                _captureService.StopCapture();
            }
            catch (Exception exception)
            {
                statusTextLabel.Text = $"释放抓包设备失败：{exception.Message}";
                SetCaptureState(false);
                return;
            }

            DrainPendingPackets(MaximumPacketsPerRefresh);
            DiscardPendingPackets();
            SetCaptureState(false);
            statusTextLabel.Text = e.CompletedWithoutError ? "抓包已停止" : "抓包异常停止";
        });
    }

    private void HandleCaptureFailed(object? sender, CaptureFailedEventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        BeginInvoke(() => statusTextLabel.Text = $"数据包处理失败：{e.Exception.Message}");
    }

    private void HandleUiTimerTick(object? sender, EventArgs e)
    {
        DrainPendingPackets(MaximumPacketsPerRefresh);
    }

    private void HandleFormClosing(object? sender, FormClosingEventArgs e)
    {
        uiTimer.Stop();
        StopCapture(showErrorDialog: false);

        _captureService.PacketCaptured -= HandlePacketCaptured;
        _captureService.CaptureStopped -= HandleCaptureStopped;
        _captureService.CaptureFailed -= HandleCaptureFailed;
        _captureService.Dispose();
    }

    private async Task RefreshDevicesAsync()
    {
        refreshButton.Enabled = false;
        startButton.Enabled = false;
        deviceComboBox.Enabled = false;
        statusTextLabel.Text = "正在扫描可用网卡...";

        try
        {
            var devices = await Task.Run(_deviceCatalog.GetDevices);
            deviceComboBox.DataSource = null;
            deviceComboBox.DisplayMember = nameof(CaptureDeviceInfo.DisplayName);
            deviceComboBox.DataSource = devices.ToList();

            var hasDevices = devices.Count > 0;
            deviceComboBox.Enabled = hasDevices;
            startButton.Enabled = hasDevices;
            statusTextLabel.Text = hasDevices
                ? $"已发现 {devices.Count} 个可用网卡"
                : "未发现可用网卡，请检查 Npcap 安装";
        }
        catch (Exception exception)
        {
            deviceComboBox.DataSource = null;
            statusTextLabel.Text = "读取网卡失败";
            ShowCaptureError("无法读取网卡列表", exception);
        }
        finally
        {
            refreshButton.Enabled = true;
        }
    }

    private void StopCapture(bool showErrorDialog)
    {
        Interlocked.Exchange(ref _acceptPackets, 0);
        uiTimer.Stop();

        try
        {
            _captureService.StopCapture();
            DrainPendingPackets(MaximumPacketsPerRefresh);
            DiscardPendingPackets();
            SetCaptureState(false);
            statusTextLabel.Text = "抓包已停止";
        }
        catch (Exception exception)
        {
            SetCaptureState(false);
            statusTextLabel.Text = "停止抓包时发生错误";

            if (showErrorDialog)
            {
                ShowCaptureError("无法正常停止抓包", exception);
            }
        }
    }

    private async Task StopCaptureAsync(bool showErrorDialog)
    {
        Interlocked.Exchange(ref _acceptPackets, 0);
        uiTimer.Stop();
        stopButton.Enabled = false;
        statusTextLabel.Text = "正在停止抓包...";

        try
        {
            await Task.Run(_captureService.StopCapture);
            DrainPendingPackets(MaximumPacketsPerRefresh);
            DiscardPendingPackets();
            SetCaptureState(false);
            statusTextLabel.Text = "抓包已停止";
        }
        catch (Exception exception)
        {
            SetCaptureState(false);
            statusTextLabel.Text = "停止抓包时发生错误";

            if (showErrorDialog)
            {
                ShowCaptureError("无法正常停止抓包", exception);
            }
        }
    }

    private void SetCaptureState(bool isCapturing)
    {
        deviceComboBox.Enabled = !isCapturing && deviceComboBox.Items.Count > 0;
        refreshButton.Enabled = !isCapturing;
        startButton.Enabled = !isCapturing && deviceComboBox.Items.Count > 0;
        stopButton.Enabled = isCapturing;
    }

    private void ClearPacketDisplay()
    {
        packetListView.Items.Clear();
        ClearPacketDetails();
        _capturedPackets.Clear();
        _trackedTcpFlow = null;
        Interlocked.Exchange(ref _capturedPacketCount, 0);
        Interlocked.Exchange(ref _droppedDisplayPacketCount, 0);

        while (_pendingPackets.TryDequeue(out _))
        {
        }

        UpdatePacketCounters();
        UpdateTcpStreamButtons();
    }

    private void AddPacketToList(PacketDisplayItem displayItem)
    {
        var packet = displayItem.Packet;
        var item = new ListViewItem(displayItem.SequenceNumber.ToString(CultureInfo.InvariantCulture));
        item.SubItems.Add(packet.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture));
        item.SubItems.Add(packet.SourceAddress);
        item.SubItems.Add(packet.DestinationAddress);
        item.SubItems.Add(packet.ProtocolName);
        item.SubItems.Add(packet.CapturedLength.ToString(CultureInfo.InvariantCulture));
        item.SubItems.Add(packet.Summary);
        item.Tag = displayItem;
        packetListView.Items.Add(item);
    }

    private void DrainPendingPackets(int maximumPackets)
    {
        packetListView.BeginUpdate();

        try
        {
            var processed = 0;
            while (processed < maximumPackets && _pendingPackets.TryDequeue(out var displayItem))
            {
                _capturedPackets.Add(displayItem);
                if (MatchesCurrentView(displayItem.Packet))
                {
                    AddPacketToList(displayItem);
                }

                processed++;
            }

            if (_capturedPackets.Count > MaximumRetainedPackets)
            {
                _capturedPackets.RemoveRange(0, _capturedPackets.Count - MaximumRetainedPackets);
            }

            while (packetListView.Items.Count > MaximumVisiblePackets)
            {
                packetListView.Items.RemoveAt(0);
            }
        }
        finally
        {
            packetListView.EndUpdate();
        }

        UpdatePacketCounters();
    }

    private void DiscardPendingPackets()
    {
        while (_pendingPackets.TryDequeue(out _))
        {
            Interlocked.Increment(ref _droppedDisplayPacketCount);
        }

        UpdatePacketCounters();
    }

    private void InitializeFilterControls()
    {
        protocolFilterComboBox.DisplayMember = nameof(FilterOption.DisplayName);
        protocolFilterComboBox.DataSource = new List<FilterOption>
        {
            new(ProtocolFilter.All, "全部"),
            new(ProtocolFilter.Http, "HTTP"),
            new(ProtocolFilter.Tcp, "TCP"),
            new(ProtocolFilter.Udp, "UDP"),
            new(ProtocolFilter.IPv4, "IPv4"),
            new(ProtocolFilter.IPv6, "IPv6"),
            new(ProtocolFilter.Icmp, "ICMP"),
            new(ProtocolFilter.Arp, "ARP"),
        };
    }

    private void ApplyFilterFromControls()
    {
        var protocol = protocolFilterComboBox.SelectedItem is FilterOption option
            ? option.Protocol
            : ProtocolFilter.All;

        IPAddress? ipAddress = null;
        var ipText = ipFilterTextBox.Text.Trim();
        if (ipText.Length > 0 && !IPAddress.TryParse(ipText, out ipAddress))
        {
            MessageBox.Show(this, "请输入有效的 IPv4 或 IPv6 地址。", "IP 地址无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ushort? port = null;
        var portText = portFilterTextBox.Text.Trim();
        if (portText.Length > 0)
        {
            if (!ushort.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPort))
            {
                MessageBox.Show(this, "端口必须是 0 到 65535 之间的整数。", "端口无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            port = parsedPort;
        }

        _activeFilter = new PacketFilter(protocol, ipAddress, port);
        ApplyFilterToVisiblePackets();
    }

    private void ApplyFilterToVisiblePackets()
    {
        packetListView.BeginUpdate();
        packetListView.Items.Clear();
        ClearPacketDetails();

        try
        {
            foreach (var displayItem in _capturedPackets
                         .Where(item => MatchesCurrentView(item.Packet))
                         .TakeLast(MaximumVisiblePackets))
            {
                AddPacketToList(displayItem);
            }
        }
        finally
        {
            packetListView.EndUpdate();
        }

        UpdatePacketCounters();
        UpdateTcpStreamButtons();
    }

    private bool MatchesCurrentView(PacketRecord packet)
    {
        return _activeFilter.Matches(packet) &&
               (_trackedTcpFlow is not TcpFlowKey flowKey || flowKey.Matches(packet));
    }

    private void DisplaySelectedPacket()
    {
        if (!TryGetSelectedPacket(out var displayItem))
        {
            ClearPacketDetails();
            return;
        }

        var detailNodes = _packetDetailParser.Parse(displayItem.Packet);
        packetDetailTreeView.BeginUpdate();

        try
        {
            packetDetailTreeView.Nodes.Clear();
            foreach (var detailNode in detailNodes)
            {
                var treeNode = CreateTreeNode(detailNode);
                packetDetailTreeView.Nodes.Add(treeNode);
                treeNode.Expand();
            }
        }
        finally
        {
            packetDetailTreeView.EndUpdate();
        }

        hexDumpTextBox.Text = HexDumpFormatter.Format(displayItem.Packet.RawData);
        hexDumpTextBox.SelectionStart = 0;
        hexDumpTextBox.SelectionLength = 0;
    }

    private bool TryGetSelectedPacket(out PacketDisplayItem displayItem)
    {
        if (packetListView.SelectedItems.Count > 0 &&
            packetListView.SelectedItems[0].Tag is PacketDisplayItem selectedPacket)
        {
            displayItem = selectedPacket;
            return true;
        }

        displayItem = null!;
        return false;
    }

    private void UpdateTcpStreamButtons()
    {
        followTcpStreamButton.Enabled =
            TryGetSelectedPacket(out var selectedPacket) &&
            TcpFlowKey.TryCreate(selectedPacket.Packet, out _);
        clearTcpStreamButton.Enabled = _trackedTcpFlow.HasValue;
    }

    private static TreeNode CreateTreeNode(PacketDetailNode detailNode)
    {
        var treeNode = new TreeNode(detailNode.DisplayText);
        foreach (var child in detailNode.Children)
        {
            treeNode.Nodes.Add(CreateTreeNode(child));
        }

        return treeNode;
    }

    private void ClearPacketDetails()
    {
        packetDetailTreeView.Nodes.Clear();
        hexDumpTextBox.Clear();
    }

    private void ApplyInitialSplitterPositions()
    {
        var mainMaximum = mainSplitContainer.Height -
                          mainSplitContainer.Panel2MinSize -
                          mainSplitContainer.SplitterWidth;
        if (mainMaximum >= mainSplitContainer.Panel1MinSize)
        {
            mainSplitContainer.SplitterDistance = Math.Clamp(
                (int)(mainSplitContainer.Height * 0.62),
                mainSplitContainer.Panel1MinSize,
                mainMaximum);
        }

        var detailMaximum = detailSplitContainer.Width -
                            detailSplitContainer.Panel2MinSize -
                            detailSplitContainer.SplitterWidth;
        if (detailMaximum >= detailSplitContainer.Panel1MinSize)
        {
            detailSplitContainer.SplitterDistance = Math.Clamp(
                detailSplitContainer.Width / 2,
                detailSplitContainer.Panel1MinSize,
                detailMaximum);
        }
    }

    private void UpdatePacketCounters()
    {
        var captured = Interlocked.Read(ref _capturedPacketCount);
        var dropped = Interlocked.Read(ref _droppedDisplayPacketCount);
        packetCountLabel.Text = dropped == 0
            ? $"已捕获：{captured:N0}，显示：{packetListView.Items.Count:N0}"
            : $"已捕获：{captured:N0}，显示：{packetListView.Items.Count:N0}，界面丢弃：{dropped:N0}";
        var retainedCount = _capturedPackets.Count(item => MatchesCurrentView(item.Packet));
        filterResultLabel.Text = _trackedTcpFlow is TcpFlowKey flowKey
            ? $"TCP 流：{flowKey.DisplayName}，保留：{retainedCount:N0}"
            : $"保留：{retainedCount:N0}";
    }

    private void ShowCaptureError(string title, Exception exception)
    {
        MessageBox.Show(
            this,
            exception.Message,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private sealed record PacketDisplayItem(long SequenceNumber, PacketRecord Packet);

    private sealed record FilterOption(ProtocolFilter Protocol, string DisplayName);
}

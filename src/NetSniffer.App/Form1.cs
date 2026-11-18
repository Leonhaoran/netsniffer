using System.Collections.Concurrent;
using System.Globalization;
using NetSniffer.Core.Models;
using NetSniffer.Core.Parsing;
using NetSniffer.Core.Services;

namespace NetSniffer.App;

public partial class Form1 : Form
{
    private const int MaximumVisiblePackets = 2_000;
    private const int MaximumPendingPackets = 5_000;
    private const int MaximumPacketsPerRefresh = 250;

    private readonly ICaptureDeviceCatalog _deviceCatalog;
    private readonly IPacketCaptureService _captureService;
    private readonly IPacketParser _packetParser;
    private readonly ConcurrentQueue<PacketDisplayItem> _pendingPackets = new();
    private long _capturedPacketCount;
    private long _droppedDisplayPacketCount;
    private int _pendingPacketCount;

    public Form1()
        : this(new SharpPcapDeviceCatalog(), new SharpPcapCaptureService(), new PacketParser())
    {
    }

    internal Form1(
        ICaptureDeviceCatalog deviceCatalog,
        IPacketCaptureService captureService,
        IPacketParser packetParser)
    {
        _deviceCatalog = deviceCatalog;
        _captureService = captureService;
        _packetParser = packetParser;

        InitializeComponent();

        Shown += HandleShown;
        FormClosing += HandleFormClosing;
        refreshButton.Click += HandleRefreshClick;
        startButton.Click += HandleStartClick;
        stopButton.Click += HandleStopClick;
        clearButton.Click += HandleClearClick;
        uiTimer.Tick += HandleUiTimerTick;

        _captureService.PacketCaptured += HandlePacketCaptured;
        _captureService.CaptureStopped += HandleCaptureStopped;
        _captureService.CaptureFailed += HandleCaptureFailed;
    }

    private async void HandleShown(object? sender, EventArgs e)
    {
        uiTimer.Start();
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
            _captureService.Start(selectedDevice.Name);
            SetCaptureState(true);
            statusTextLabel.Text = $"正在抓包：{selectedDevice.Description}";
        }
        catch (Exception exception)
        {
            SetCaptureState(false);
            ShowCaptureError("无法开始抓包", exception);
        }
    }

    private void HandleStopClick(object? sender, EventArgs e)
    {
        StopCapture(showErrorDialog: true);
    }

    private void HandleClearClick(object? sender, EventArgs e)
    {
        ClearPacketDisplay();
    }

    private void HandlePacketCaptured(object? sender, PacketCapturedEventArgs e)
    {
        var sequenceNumber = Interlocked.Increment(ref _capturedPacketCount);
        var parsedPacket = _packetParser.Parse(e.Packet);
        var pendingCount = Interlocked.Increment(ref _pendingPacketCount);

        if (pendingCount > MaximumPendingPackets)
        {
            Interlocked.Decrement(ref _pendingPacketCount);
            Interlocked.Increment(ref _droppedDisplayPacketCount);
            return;
        }

        _pendingPackets.Enqueue(new PacketDisplayItem(sequenceNumber, parsedPacket));
    }

    private void HandleCaptureStopped(object? sender, CaptureStoppedEventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        BeginInvoke(() =>
        {
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
        packetListView.BeginUpdate();

        try
        {
            var processed = 0;
            while (processed < MaximumPacketsPerRefresh &&
                   _pendingPackets.TryDequeue(out var displayItem))
            {
                Interlocked.Decrement(ref _pendingPacketCount);
                AddPacketToList(displayItem);
                processed++;
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

        var captured = Interlocked.Read(ref _capturedPacketCount);
        var dropped = Interlocked.Read(ref _droppedDisplayPacketCount);
        packetCountLabel.Text = dropped == 0
            ? $"已捕获：{captured:N0}"
            : $"已捕获：{captured:N0}，界面丢弃：{dropped:N0}";
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
        try
        {
            _captureService.StopCapture();
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
        Interlocked.Exchange(ref _capturedPacketCount, 0);
        Interlocked.Exchange(ref _droppedDisplayPacketCount, 0);

        while (_pendingPackets.TryDequeue(out _))
        {
        }

        Interlocked.Exchange(ref _pendingPacketCount, 0);
        packetCountLabel.Text = "已捕获：0";
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
        packetListView.Items.Add(item);
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
}

using NetSniffer.Core.Models;
using SharpPcap;

namespace NetSniffer.Core.Services;

public sealed class SharpPcapCaptureService : IPacketCaptureService
{
    private readonly object _syncRoot = new();
    private ILiveDevice? _activeDevice;
    private bool _disposed;

    public event EventHandler<PacketCapturedEventArgs>? PacketCaptured;

    public event EventHandler<CaptureStoppedEventArgs>? CaptureStopped;

    public event EventHandler<CaptureFailedEventArgs>? CaptureFailed;

    public bool IsCapturing
    {
        get
        {
            lock (_syncRoot)
            {
                return _activeDevice?.Started == true;
            }
        }
    }

    public void Start(string deviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceName);

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_activeDevice is not null)
            {
                throw new InvalidOperationException("抓包已经在运行。");
            }

            var device = CaptureDeviceList.Instance[deviceName]
                ?? throw new InvalidOperationException("所选网卡已经不可用，请刷新网卡列表。");

            device.OnPacketArrival += HandlePacketArrival;
            device.OnCaptureStopped += HandleCaptureStopped;

            try
            {
                device.Open(new DeviceConfiguration
                {
                    Mode = DeviceModes.Promiscuous,
                    ReadTimeout = 1000,
                    Snaplen = 65_535,
                    Immediate = true,
                });

                device.StartCapture();
                _activeDevice = device;
            }
            catch
            {
                device.OnPacketArrival -= HandlePacketArrival;
                device.OnCaptureStopped -= HandleCaptureStopped;
                device.Close();
                device.Dispose();
                throw;
            }
        }
    }

    public void StopCapture()
    {
        ILiveDevice? device;

        lock (_syncRoot)
        {
            device = _activeDevice;
            _activeDevice = null;
        }

        if (device is null)
        {
            return;
        }

        try
        {
            if (device.Started)
            {
                device.StopCapture();
            }
        }
        finally
        {
            device.OnPacketArrival -= HandlePacketArrival;
            device.OnCaptureStopped -= HandleCaptureStopped;
            device.Close();
            device.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        StopCapture();
    }

    private void HandlePacketArrival(object sender, PacketCapture packetCapture)
    {
        try
        {
            var rawPacket = packetCapture.GetPacket();
            var packet = new CapturedPacket(
                rawPacket.Timeval.Date,
                rawPacket.Data.Length,
                rawPacket.PacketLength,
                rawPacket.LinkLayerType,
                rawPacket.Data.ToArray());

            PacketCaptured?.Invoke(this, new PacketCapturedEventArgs(packet));
        }
        catch (Exception exception)
        {
            CaptureFailed?.Invoke(this, new CaptureFailedEventArgs(exception));
        }
    }

    private void HandleCaptureStopped(object sender, CaptureStoppedEventStatus status)
    {
        CaptureStopped?.Invoke(
            this,
            new CaptureStoppedEventArgs(status == CaptureStoppedEventStatus.CompletedWithoutError));
    }
}


namespace NetSniffer.Core.Services;

public interface IPacketCaptureService : IDisposable
{
    event EventHandler<PacketCapturedEventArgs>? PacketCaptured;

    event EventHandler<CaptureStoppedEventArgs>? CaptureStopped;

    event EventHandler<CaptureFailedEventArgs>? CaptureFailed;

    bool IsCapturing { get; }

    void Start(string deviceName);

    void StopCapture();
}


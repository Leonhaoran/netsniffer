namespace NetSniffer.Core.Services;

public sealed class CaptureStoppedEventArgs(bool completedWithoutError) : EventArgs
{
    public bool CompletedWithoutError { get; } = completedWithoutError;
}


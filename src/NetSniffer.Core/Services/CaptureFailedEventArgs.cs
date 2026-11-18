namespace NetSniffer.Core.Services;

public sealed class CaptureFailedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception { get; } = exception;
}


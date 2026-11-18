namespace NetSniffer.Core.Models;

/// <summary>
/// Raw packet data copied from the native capture buffer.
/// </summary>
public sealed record CapturedPacket(
    DateTime Timestamp,
    int CapturedLength,
    int OriginalLength,
    string LinkLayerType,
    byte[] Data);


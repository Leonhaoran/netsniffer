using NetSniffer.Core.Models;

namespace NetSniffer.Core.Services;

public sealed class PacketCapturedEventArgs(CapturedPacket packet) : EventArgs
{
    public CapturedPacket Packet { get; } = packet;
}


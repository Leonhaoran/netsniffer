namespace NetSniffer.Core.Models;

public sealed record PacketRecord(
    DateTime Timestamp,
    string SourceAddress,
    string DestinationAddress,
    ushort? SourcePort,
    ushort? DestinationPort,
    PacketProtocol Protocol,
    PacketNetworkLayer NetworkLayer,
    int CapturedLength,
    int OriginalLength,
    string Summary,
    string LinkLayerType,
    byte[] RawData)
{
    public string ProtocolName => Protocol switch
    {
        PacketProtocol.Arp => "ARP",
        PacketProtocol.IPv4 => "IPv4",
        PacketProtocol.IPv6 => "IPv6",
        PacketProtocol.Tcp => "TCP",
        PacketProtocol.Udp => "UDP",
        PacketProtocol.Icmp => "ICMP",
        PacketProtocol.IcmpV6 => "ICMPv6",
        PacketProtocol.Http => "HTTP",
        PacketProtocol.Ethernet => "Ethernet",
        _ => "Unknown",
    };
}


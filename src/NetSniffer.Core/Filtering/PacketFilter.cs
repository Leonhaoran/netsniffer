using System.Net;
using NetSniffer.Core.Models;

namespace NetSniffer.Core.Filtering;

public sealed record PacketFilter(
    ProtocolFilter Protocol,
    IPAddress? IpAddress,
    ushort? Port)
{
    public static PacketFilter All { get; } = new(ProtocolFilter.All, null, null);

    public bool Matches(PacketRecord packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        return MatchesProtocol(packet) && MatchesIpAddress(packet) && MatchesPort(packet);
    }

    private bool MatchesProtocol(PacketRecord packet)
    {
        return Protocol switch
        {
            ProtocolFilter.All => true,
            ProtocolFilter.Http => packet.Protocol == PacketProtocol.Http,
            ProtocolFilter.Tcp => packet.Protocol is PacketProtocol.Tcp or PacketProtocol.Http,
            ProtocolFilter.Udp => packet.Protocol == PacketProtocol.Udp,
            ProtocolFilter.IPv4 => packet.NetworkLayer == PacketNetworkLayer.IPv4,
            ProtocolFilter.IPv6 => packet.NetworkLayer == PacketNetworkLayer.IPv6,
            ProtocolFilter.Icmp => packet.Protocol is PacketProtocol.Icmp or PacketProtocol.IcmpV6,
            ProtocolFilter.Arp => packet.Protocol == PacketProtocol.Arp,
            _ => false,
        };
    }

    private bool MatchesIpAddress(PacketRecord packet)
    {
        if (IpAddress is null)
        {
            return true;
        }

        return IsSameIpAddress(packet.SourceAddress, IpAddress) ||
               IsSameIpAddress(packet.DestinationAddress, IpAddress);
    }

    private bool MatchesPort(PacketRecord packet)
    {
        return Port is null || packet.SourcePort == Port || packet.DestinationPort == Port;
    }

    private static bool IsSameIpAddress(string value, IPAddress expected)
    {
        return IPAddress.TryParse(value, out var parsed) && parsed.Equals(expected);
    }
}


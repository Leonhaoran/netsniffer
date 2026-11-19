using NetSniffer.Core.Models;

namespace NetSniffer.Core.Flows;

public readonly record struct TcpFlowKey(TcpEndpoint EndpointA, TcpEndpoint EndpointB)
{
    public string DisplayName => $"{EndpointA} ↔ {EndpointB}";

    public static bool TryCreate(PacketRecord packet, out TcpFlowKey flowKey)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.Protocol is not (PacketProtocol.Tcp or PacketProtocol.Http) ||
            packet.SourcePort is not ushort sourcePort ||
            packet.DestinationPort is not ushort destinationPort ||
            string.IsNullOrWhiteSpace(packet.SourceAddress) ||
            string.IsNullOrWhiteSpace(packet.DestinationAddress))
        {
            flowKey = default;
            return false;
        }

        var source = new TcpEndpoint(packet.SourceAddress, sourcePort);
        var destination = new TcpEndpoint(packet.DestinationAddress, destinationPort);
        flowKey = Compare(source, destination) <= 0
            ? new TcpFlowKey(source, destination)
            : new TcpFlowKey(destination, source);
        return true;
    }

    public bool Matches(PacketRecord packet)
    {
        return TryCreate(packet, out var candidate) && candidate == this;
    }

    private static int Compare(TcpEndpoint left, TcpEndpoint right)
    {
        var addressComparison = StringComparer.OrdinalIgnoreCase.Compare(left.Address, right.Address);
        return addressComparison != 0 ? addressComparison : left.Port.CompareTo(right.Port);
    }
}

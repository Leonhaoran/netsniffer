using NetSniffer.Core.Models;

namespace NetSniffer.Core.Details;

public interface IPacketDetailParser
{
    IReadOnlyList<PacketDetailNode> Parse(PacketRecord packetRecord);
}


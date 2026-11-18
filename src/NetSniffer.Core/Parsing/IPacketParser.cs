using NetSniffer.Core.Models;

namespace NetSniffer.Core.Parsing;

public interface IPacketParser
{
    PacketRecord Parse(CapturedPacket capturedPacket);
}


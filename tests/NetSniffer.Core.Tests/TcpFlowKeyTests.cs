using NetSniffer.Core.Flows;
using NetSniffer.Core.Models;

namespace NetSniffer.Core.Tests;

public class TcpFlowKeyTests
{
    [Fact]
    public void ReversePacketsProduceTheSameFlowKey()
    {
        var outbound = CreatePacket("192.168.1.10", 51_000, "93.184.216.34", 443, PacketProtocol.Tcp);
        var inbound = CreatePacket("93.184.216.34", 443, "192.168.1.10", 51_000, PacketProtocol.Tcp);

        Assert.True(TcpFlowKey.TryCreate(outbound, out var outboundKey));
        Assert.True(TcpFlowKey.TryCreate(inbound, out var inboundKey));
        Assert.Equal(outboundKey, inboundKey);
        Assert.True(outboundKey.Matches(inbound));
    }

    [Fact]
    public void DifferentPortDoesNotMatchTheTrackedFlow()
    {
        var selected = CreatePacket("10.0.0.2", 50_000, "10.0.0.1", 80, PacketProtocol.Http);
        var otherConnection = CreatePacket("10.0.0.2", 50_001, "10.0.0.1", 80, PacketProtocol.Tcp);

        Assert.True(TcpFlowKey.TryCreate(selected, out var flowKey));
        Assert.False(flowKey.Matches(otherConnection));
    }

    [Fact]
    public void UdpPacketCannotCreateTcpFlow()
    {
        var udpPacket = CreatePacket("2001:db8::1", 53_000, "2001:db8::2", 53, PacketProtocol.Udp);

        Assert.False(TcpFlowKey.TryCreate(udpPacket, out _));
    }

    private static PacketRecord CreatePacket(
        string sourceAddress,
        ushort sourcePort,
        string destinationAddress,
        ushort destinationPort,
        PacketProtocol protocol)
    {
        return new PacketRecord(
            DateTime.UtcNow,
            sourceAddress,
            destinationAddress,
            sourcePort,
            destinationPort,
            protocol,
            sourceAddress.Contains(':') ? PacketNetworkLayer.IPv6 : PacketNetworkLayer.IPv4,
            64,
            64,
            "test",
            "Ethernet",
            []);
    }
}

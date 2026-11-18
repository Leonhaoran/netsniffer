using System.Net;
using NetSniffer.Core.Filtering;
using NetSniffer.Core.Models;

namespace NetSniffer.Core.Tests;

public class PacketFilterTests
{
    [Fact]
    public void TcpFilterIncludesHttpPackets()
    {
        var packet = CreatePacket(PacketProtocol.Http, PacketNetworkLayer.IPv4, 51_000, 80);
        var filter = new PacketFilter(ProtocolFilter.Tcp, null, null);

        Assert.True(filter.Matches(packet));
    }

    [Fact]
    public void NetworkLayerFilterMatchesTransportPacket()
    {
        var packet = CreatePacket(PacketProtocol.Tcp, PacketNetworkLayer.IPv6, 51_000, 443);
        var filter = new PacketFilter(ProtocolFilter.IPv6, null, null);

        Assert.True(filter.Matches(packet));
    }

    [Fact]
    public void AddressAndPortFiltersAreCombined()
    {
        var packet = CreatePacket(PacketProtocol.Udp, PacketNetworkLayer.IPv4, 53_000, 53);
        var matchingFilter = new PacketFilter(
            ProtocolFilter.Udp,
            IPAddress.Parse("198.51.100.20"),
            53);
        var wrongPortFilter = matchingFilter with { Port = 443 };

        Assert.True(matchingFilter.Matches(packet));
        Assert.False(wrongPortFilter.Matches(packet));
    }

    private static PacketRecord CreatePacket(
        PacketProtocol protocol,
        PacketNetworkLayer networkLayer,
        ushort sourcePort,
        ushort destinationPort)
    {
        return new PacketRecord(
            DateTime.UtcNow,
            "192.0.2.10",
            "198.51.100.20",
            sourcePort,
            destinationPort,
            protocol,
            networkLayer,
            128,
            128,
            "test packet",
            "Ethernet",
            []);
    }
}


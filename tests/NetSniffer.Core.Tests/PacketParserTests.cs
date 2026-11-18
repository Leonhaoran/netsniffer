using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using NetSniffer.Core.Models;
using NetSniffer.Core.Parsing;
using PacketDotNet;

namespace NetSniffer.Core.Tests;

public class PacketParserTests
{
    private readonly PacketParser _parser = new();

    [Fact]
    public void ParseRecognizesTcpPacketAndEndpoints()
    {
        var capturedPacket = CreateTcpPacket(12_345, 443);

        var result = _parser.Parse(capturedPacket);

        Assert.Equal(PacketProtocol.Tcp, result.Protocol);
        Assert.Equal("192.0.2.10", result.SourceAddress);
        Assert.Equal("198.51.100.20", result.DestinationAddress);
        Assert.Equal((ushort)12_345, result.SourcePort);
        Assert.Equal((ushort)443, result.DestinationPort);
    }

    [Fact]
    public void ParseRecognizesHttpRequestFromTcpPayload()
    {
        var payload = Encoding.ASCII.GetBytes("GET /demo HTTP/1.1\r\nHost: example.test\r\n\r\n");
        var capturedPacket = CreateTcpPacket(51_000, 80, payload);

        var result = _parser.Parse(capturedPacket);

        Assert.Equal(PacketProtocol.Http, result.Protocol);
        Assert.Equal("GET /demo HTTP/1.1", result.Summary);
    }

    [Fact]
    public void ParseRecognizesUdpPacketAndPorts()
    {
        var capturedPacket = CreateUdpPacket(53_000, 53);

        var result = _parser.Parse(capturedPacket);

        Assert.Equal(PacketProtocol.Udp, result.Protocol);
        Assert.Equal((ushort)53_000, result.SourcePort);
        Assert.Equal((ushort)53, result.DestinationPort);
    }

    [Fact]
    public void ParseReturnsUnknownForTruncatedEthernetPacket()
    {
        var bytes = new byte[] { 0x00, 0x01, 0x02 };
        var capturedPacket = new CapturedPacket(
            DateTime.UtcNow,
            bytes.Length,
            bytes.Length,
            LinkLayers.Ethernet,
            bytes);

        var result = _parser.Parse(capturedPacket);

        Assert.Equal(PacketProtocol.Unknown, result.Protocol);
    }

    private static CapturedPacket CreateTcpPacket(
        ushort sourcePort,
        ushort destinationPort,
        byte[]? payload = null)
    {
        var tcpPacket = new TcpPacket(sourcePort, destinationPort)
        {
            Synchronize = payload is null,
            Acknowledgment = payload is not null,
            PayloadData = payload ?? [],
        };

        return CreateIpPacket(tcpPacket);
    }

    private static CapturedPacket CreateUdpPacket(ushort sourcePort, ushort destinationPort)
    {
        return CreateIpPacket(new UdpPacket(sourcePort, destinationPort));
    }

    private static CapturedPacket CreateIpPacket(Packet transportPacket)
    {
        var ipPacket = new IPv4Packet(
            IPAddress.Parse("192.0.2.10"),
            IPAddress.Parse("198.51.100.20"))
        {
            PayloadPacket = transportPacket,
        };
        var ethernetPacket = new EthernetPacket(
            PhysicalAddress.Parse("001122334455"),
            PhysicalAddress.Parse("AABBCCDDEEFF"),
            EthernetType.IPv4)
        {
            PayloadPacket = ipPacket,
        };

        transportPacket.UpdateCalculatedValues();
        ipPacket.UpdateCalculatedValues();
        ethernetPacket.UpdateCalculatedValues();
        var bytes = ethernetPacket.Bytes;

        return new CapturedPacket(
            DateTime.UtcNow,
            bytes.Length,
            bytes.Length,
            LinkLayers.Ethernet,
            bytes);
    }
}


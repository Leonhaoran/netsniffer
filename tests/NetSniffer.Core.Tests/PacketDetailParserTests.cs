using System.Net;
using System.Net.NetworkInformation;
using NetSniffer.Core.Details;
using NetSniffer.Core.Models;
using PacketDotNet;

namespace NetSniffer.Core.Tests;

public class PacketDetailParserTests
{
    [Fact]
    public void ParseBuildsEthernetIpAndTcpLayers()
    {
        var tcpPacket = new TcpPacket(12_345, 443)
        {
            Synchronize = true,
        };
        var ipPacket = new IPv4Packet(
            IPAddress.Parse("192.0.2.10"),
            IPAddress.Parse("198.51.100.20"))
        {
            PayloadPacket = tcpPacket,
        };
        var ethernetPacket = new EthernetPacket(
            PhysicalAddress.Parse("001122334455"),
            PhysicalAddress.Parse("AABBCCDDEEFF"),
            EthernetType.IPv4)
        {
            PayloadPacket = ipPacket,
        };
        tcpPacket.UpdateCalculatedValues();
        ipPacket.UpdateCalculatedValues();
        ethernetPacket.UpdateCalculatedValues();
        var bytes = ethernetPacket.Bytes;
        var record = new PacketRecord(
            DateTime.UtcNow,
            "192.0.2.10",
            "198.51.100.20",
            12_345,
            443,
            PacketProtocol.Tcp,
            PacketNetworkLayer.IPv4,
            bytes.Length,
            bytes.Length,
            "TCP SYN",
            LinkLayers.Ethernet.ToString(),
            bytes);

        var result = new PacketDetailParser().Parse(record);

        Assert.Contains(result, node => node.Name == "Ethernet II");
        var ipv4Node = Assert.Single(result, node => node.Name == "Internet Protocol Version 4");
        Assert.Contains(ipv4Node.Children, node => node.DisplayText == "Header length: 20 bytes");
        var tcpNode = Assert.Single(result, node => node.Name == "Transmission Control Protocol");
        Assert.Contains(tcpNode.Children, node => node.DisplayText.Contains("12345", StringComparison.Ordinal));
        Assert.Contains(tcpNode.Children, node => node.DisplayText.Contains("SYN", StringComparison.Ordinal));
    }
}


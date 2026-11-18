using System.Text;
using NetSniffer.Core.Models;
using PacketDotNet;

namespace NetSniffer.Core.Parsing;

public sealed class PacketParser : IPacketParser
{
    private static readonly string[] HttpRequestPrefixes =
    [
        "GET ",
        "POST ",
        "PUT ",
        "DELETE ",
        "HEAD ",
        "OPTIONS ",
        "PATCH ",
        "CONNECT ",
        "TRACE ",
    ];

    public PacketRecord Parse(CapturedPacket capturedPacket)
    {
        ArgumentNullException.ThrowIfNull(capturedPacket);

        try
        {
            var packet = Packet.ParsePacket(capturedPacket.LinkLayerType, capturedPacket.Data);
            return CreatePacketRecord(capturedPacket, packet);
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException or IndexOutOfRangeException)
        {
            return CreateUnknownRecord(capturedPacket, $"无法解析的数据包 ({exception.GetType().Name})");
        }
    }

    private static PacketRecord CreatePacketRecord(CapturedPacket capturedPacket, Packet packet)
    {
        var ethernetPacket = packet.Extract<EthernetPacket>();
        var ipPacket = packet.Extract<IPPacket>();
        var tcpPacket = packet.Extract<TcpPacket>();
        var udpPacket = packet.Extract<UdpPacket>();
        var icmpV4Packet = packet.Extract<IcmpV4Packet>();
        var icmpV6Packet = packet.Extract<IcmpV6Packet>();
        var arpPacket = packet.Extract<ArpPacket>();

        var sourceAddress = ipPacket?.SourceAddress.ToString()
            ?? arpPacket?.SenderProtocolAddress.ToString()
            ?? ethernetPacket?.SourceHardwareAddress.ToString()
            ?? "-";
        var destinationAddress = ipPacket?.DestinationAddress.ToString()
            ?? arpPacket?.TargetProtocolAddress.ToString()
            ?? ethernetPacket?.DestinationHardwareAddress.ToString()
            ?? "-";

        var sourcePort = tcpPacket?.SourcePort ?? udpPacket?.SourcePort;
        var destinationPort = tcpPacket?.DestinationPort ?? udpPacket?.DestinationPort;

        var (protocol, summary) = ClassifyPacket(
            ipPacket,
            tcpPacket,
            udpPacket,
            icmpV4Packet,
            icmpV6Packet,
            arpPacket,
            ethernetPacket);

        return new PacketRecord(
            capturedPacket.Timestamp,
            sourceAddress,
            destinationAddress,
            sourcePort,
            destinationPort,
            protocol,
            GetNetworkLayer(ipPacket, arpPacket),
            capturedPacket.CapturedLength,
            capturedPacket.OriginalLength,
            summary,
            capturedPacket.LinkLayerType.ToString(),
            capturedPacket.Data);
    }

    private static PacketNetworkLayer GetNetworkLayer(IPPacket? ipPacket, ArpPacket? arpPacket)
    {
        if (arpPacket is not null)
        {
            return PacketNetworkLayer.Arp;
        }

        return ipPacket switch
        {
            IPv4Packet => PacketNetworkLayer.IPv4,
            IPv6Packet => PacketNetworkLayer.IPv6,
            _ => PacketNetworkLayer.Unknown,
        };
    }

    private static (PacketProtocol Protocol, string Summary) ClassifyPacket(
        IPPacket? ipPacket,
        TcpPacket? tcpPacket,
        UdpPacket? udpPacket,
        IcmpV4Packet? icmpV4Packet,
        IcmpV6Packet? icmpV6Packet,
        ArpPacket? arpPacket,
        EthernetPacket? ethernetPacket)
    {
        if (tcpPacket is not null)
        {
            var httpSummary = TryGetHttpSummary(tcpPacket.PayloadData);
            if (httpSummary is not null)
            {
                return (PacketProtocol.Http, httpSummary);
            }

            return (PacketProtocol.Tcp, BuildTcpSummary(tcpPacket));
        }

        if (udpPacket is not null)
        {
            return (
                PacketProtocol.Udp,
                $"{udpPacket.SourcePort} → {udpPacket.DestinationPort}, UDP 长度 {udpPacket.Length}");
        }

        if (icmpV4Packet is not null)
        {
            return (
                PacketProtocol.Icmp,
                $"{icmpV4Packet.TypeCode}, ID {icmpV4Packet.Id}, 序号 {icmpV4Packet.Sequence}");
        }

        if (icmpV6Packet is not null)
        {
            return (
                PacketProtocol.IcmpV6,
                $"{icmpV6Packet.Type}, Code {icmpV6Packet.Code}");
        }

        if (arpPacket is not null)
        {
            return (
                PacketProtocol.Arp,
                $"{arpPacket.Operation}: {arpPacket.SenderProtocolAddress} → {arpPacket.TargetProtocolAddress}");
        }

        if (ipPacket is IPv4Packet)
        {
            return (PacketProtocol.IPv4, $"IPv4, 上层协议 {ipPacket.Protocol}");
        }

        if (ipPacket is IPv6Packet)
        {
            return (PacketProtocol.IPv6, $"IPv6, 上层协议 {ipPacket.Protocol}");
        }

        if (ethernetPacket is not null)
        {
            return (PacketProtocol.Ethernet, $"Ethernet 类型 {ethernetPacket.Type}");
        }

        return (PacketProtocol.Unknown, "无法识别的数据包");
    }

    private static string BuildTcpSummary(TcpPacket packet)
    {
        var flags = new List<string>();

        if (packet.Synchronize)
        {
            flags.Add("SYN");
        }

        if (packet.Acknowledgment)
        {
            flags.Add("ACK");
        }

        if (packet.Push)
        {
            flags.Add("PSH");
        }

        if (packet.Finished)
        {
            flags.Add("FIN");
        }

        if (packet.Reset)
        {
            flags.Add("RST");
        }

        var flagText = flags.Count == 0 ? string.Empty : $" [{string.Join(", ", flags)}]";
        return $"{packet.SourcePort} → {packet.DestinationPort}{flagText}, Seq={packet.SequenceNumber}, Ack={packet.AcknowledgmentNumber}";
    }

    private static string? TryGetHttpSummary(byte[] payload)
    {
        if (payload.Length == 0)
        {
            return null;
        }

        var previewLength = Math.Min(payload.Length, 512);
        var preview = Encoding.ASCII.GetString(payload, 0, previewLength);
        var isHttp = preview.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase) ||
                     HttpRequestPrefixes.Any(prefix =>
                         preview.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (!isHttp)
        {
            return null;
        }

        var firstLineEnd = preview.IndexOf("\r\n", StringComparison.Ordinal);
        var firstLine = firstLineEnd >= 0 ? preview[..firstLineEnd] : preview;
        firstLine = new string(firstLine.Where(character => !char.IsControl(character)).ToArray());

        return firstLine.Length <= 160 ? firstLine : $"{firstLine[..157]}...";
    }

    private static PacketRecord CreateUnknownRecord(CapturedPacket capturedPacket, string summary)
    {
        return new PacketRecord(
            capturedPacket.Timestamp,
            "-",
            "-",
            null,
            null,
            PacketProtocol.Unknown,
            PacketNetworkLayer.Unknown,
            capturedPacket.CapturedLength,
            capturedPacket.OriginalLength,
            summary,
            capturedPacket.LinkLayerType.ToString(),
            capturedPacket.Data);
    }
}


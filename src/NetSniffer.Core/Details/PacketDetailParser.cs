using System.Globalization;
using System.Text;
using NetSniffer.Core.Models;
using PacketDotNet;

namespace NetSniffer.Core.Details;

public sealed class PacketDetailParser : IPacketDetailParser
{
    public IReadOnlyList<PacketDetailNode> Parse(PacketRecord packetRecord)
    {
        ArgumentNullException.ThrowIfNull(packetRecord);

        var nodes = new List<PacketDetailNode>
        {
            CreateFrameNode(packetRecord),
        };

        if (!Enum.TryParse<LinkLayers>(packetRecord.LinkLayerType, out var linkLayerType))
        {
            nodes.Add(new PacketDetailNode("解析错误", $"不支持的链路层类型 {packetRecord.LinkLayerType}"));
            return nodes;
        }

        try
        {
            var packet = Packet.ParsePacket(linkLayerType, packetRecord.RawData);
            AddEthernetNode(nodes, packet.Extract<EthernetPacket>());
            AddArpNode(nodes, packet.Extract<ArpPacket>());
            AddIpNode(nodes, packet.Extract<IPPacket>());
            AddTcpNode(nodes, packet.Extract<TcpPacket>());
            AddUdpNode(nodes, packet.Extract<UdpPacket>());
            AddIcmpV4Node(nodes, packet.Extract<IcmpV4Packet>());
            AddIcmpV6Node(nodes, packet.Extract<IcmpV6Packet>());
            AddHttpNode(nodes, packetRecord, packet.Extract<TcpPacket>());
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException or IndexOutOfRangeException)
        {
            nodes.Add(new PacketDetailNode("解析错误", exception.GetType().Name));
        }

        return nodes;
    }

    private static PacketDetailNode CreateFrameNode(PacketRecord packet)
    {
        return new PacketDetailNode(
            "Frame",
            $"{packet.CapturedLength} bytes on wire",
            [
                new("捕获时间", packet.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CurrentCulture)),
                new("捕获长度", $"{packet.CapturedLength} bytes"),
                new("原始长度", $"{packet.OriginalLength} bytes"),
                new("链路层类型", packet.LinkLayerType),
            ]);
    }

    private static void AddEthernetNode(List<PacketDetailNode> nodes, EthernetPacket? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "Ethernet II",
            $"Src: {packet.SourceHardwareAddress}, Dst: {packet.DestinationHardwareAddress}",
            [
                new("Destination", packet.DestinationHardwareAddress.ToString()),
                new("Source", packet.SourceHardwareAddress.ToString()),
                new("Type", $"{packet.Type} (0x{(ushort)packet.Type:X4})"),
            ]));
    }

    private static void AddArpNode(List<PacketDetailNode> nodes, ArpPacket? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "Address Resolution Protocol",
            packet.Operation.ToString(),
            [
                new("Hardware type", packet.HardwareAddressType.ToString()),
                new("Protocol type", packet.ProtocolAddressType.ToString()),
                new("Operation", packet.Operation.ToString()),
                new("Sender MAC address", packet.SenderHardwareAddress.ToString()),
                new("Sender IP address", packet.SenderProtocolAddress.ToString()),
                new("Target MAC address", packet.TargetHardwareAddress.ToString()),
                new("Target IP address", packet.TargetProtocolAddress.ToString()),
            ]));
    }

    private static void AddIpNode(List<PacketDetailNode> nodes, IPPacket? packet)
    {
        if (packet is null)
        {
            return;
        }

        var children = new List<PacketDetailNode>
        {
            new("Version", packet.Version.ToString()),
            new("Header length", $"{packet.HeaderLength * 4} bytes"),
            new("Total length", $"{packet.TotalLength} bytes"),
            new("Time to live / Hop limit", packet.TimeToLive.ToString(CultureInfo.InvariantCulture)),
            new("Protocol", packet.Protocol.ToString()),
            new("Source address", packet.SourceAddress.ToString()),
            new("Destination address", packet.DestinationAddress.ToString()),
        };

        var title = packet is IPv6Packet ? "Internet Protocol Version 6" : "Internet Protocol Version 4";

        if (packet is IPv4Packet ipv4Packet)
        {
            children.Insert(3, new PacketDetailNode("Identification", $"0x{ipv4Packet.Id:X4} ({ipv4Packet.Id})"));
            children.Add(new PacketDetailNode("Header checksum", $"0x{ipv4Packet.Checksum:X4}"));
        }
        else if (packet is IPv6Packet ipv6Packet)
        {
            children.Insert(1, new PacketDetailNode("Traffic class", ipv6Packet.TrafficClass.ToString(CultureInfo.InvariantCulture)));
            children.Insert(2, new PacketDetailNode("Flow label", $"0x{ipv6Packet.FlowLabel:X5}"));
        }

        nodes.Add(new PacketDetailNode(
            title,
            $"Src: {packet.SourceAddress}, Dst: {packet.DestinationAddress}",
            children));
    }

    private static void AddTcpNode(List<PacketDetailNode> nodes, TcpPacket? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "Transmission Control Protocol",
            $"Src Port: {packet.SourcePort}, Dst Port: {packet.DestinationPort}",
            [
                new("Source port", packet.SourcePort.ToString(CultureInfo.InvariantCulture)),
                new("Destination port", packet.DestinationPort.ToString(CultureInfo.InvariantCulture)),
                new("Sequence number", packet.SequenceNumber.ToString(CultureInfo.InvariantCulture)),
                new("Acknowledgment number", packet.AcknowledgmentNumber.ToString(CultureInfo.InvariantCulture)),
                new("Header length", $"{packet.DataOffset * 4} bytes"),
                new("Flags", $"0x{packet.Flags:X3} ({BuildTcpFlags(packet)})"),
                new("Window size", packet.WindowSize.ToString(CultureInfo.InvariantCulture)),
                new("Checksum", $"0x{packet.Checksum:X4}"),
                new("TCP payload", $"{packet.PayloadData.Length} bytes"),
            ]));
    }

    private static void AddUdpNode(List<PacketDetailNode> nodes, UdpPacket? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "User Datagram Protocol",
            $"Src Port: {packet.SourcePort}, Dst Port: {packet.DestinationPort}",
            [
                new("Source port", packet.SourcePort.ToString(CultureInfo.InvariantCulture)),
                new("Destination port", packet.DestinationPort.ToString(CultureInfo.InvariantCulture)),
                new("Length", $"{packet.Length} bytes"),
                new("Checksum", $"0x{packet.Checksum:X4}"),
                new("UDP payload", $"{packet.PayloadData.Length} bytes"),
            ]));
    }

    private static void AddIcmpV4Node(List<PacketDetailNode> nodes, IcmpV4Packet? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "Internet Control Message Protocol",
            packet.TypeCode.ToString(),
            [
                new("Type / Code", packet.TypeCode.ToString()),
                new("Checksum", $"0x{packet.Checksum:X4}"),
                new("Identifier", packet.Id.ToString(CultureInfo.InvariantCulture)),
                new("Sequence number", packet.Sequence.ToString(CultureInfo.InvariantCulture)),
            ]));
    }

    private static void AddIcmpV6Node(List<PacketDetailNode> nodes, IcmpV6Packet? packet)
    {
        if (packet is null)
        {
            return;
        }

        nodes.Add(new PacketDetailNode(
            "Internet Control Message Protocol v6",
            packet.Type.ToString(),
            [
                new("Type", packet.Type.ToString()),
                new("Code", packet.Code.ToString(CultureInfo.InvariantCulture)),
                new("Checksum", $"0x{packet.Checksum:X4}"),
            ]));
    }

    private static void AddHttpNode(
        List<PacketDetailNode> nodes,
        PacketRecord packetRecord,
        TcpPacket? tcpPacket)
    {
        if (packetRecord.Protocol != PacketProtocol.Http || tcpPacket?.PayloadData.Length is not > 0)
        {
            return;
        }

        var text = Encoding.ASCII.GetString(tcpPacket.PayloadData);
        var lines = text.Split(["\r\n"], StringSplitOptions.None);
        var firstLine = Sanitize(lines.FirstOrDefault() ?? "HTTP");
        var headerNodes = lines
            .Skip(1)
            .TakeWhile(line => line.Length > 0)
            .Take(100)
            .Select(line => new PacketDetailNode("Header", Sanitize(line)))
            .ToArray();

        nodes.Add(new PacketDetailNode("Hypertext Transfer Protocol", firstLine, headerNodes));
    }

    private static string BuildTcpFlags(TcpPacket packet)
    {
        var flags = new List<string>();

        if (packet.Synchronize) flags.Add("SYN");
        if (packet.Acknowledgment) flags.Add("ACK");
        if (packet.Push) flags.Add("PSH");
        if (packet.Finished) flags.Add("FIN");
        if (packet.Reset) flags.Add("RST");
        if (packet.Urgent) flags.Add("URG");
        if (packet.ExplicitCongestionNotificationEcho) flags.Add("ECE");
        if (packet.CongestionWindowReduced) flags.Add("CWR");

        return flags.Count == 0 ? "None" : string.Join(", ", flags);
    }

    private static string Sanitize(string value)
    {
        var characters = value
            .Where(character => !char.IsControl(character) || character == '\t')
            .Take(300)
            .ToArray();
        return new string(characters);
    }
}


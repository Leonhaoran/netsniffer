namespace NetSniffer.Core.Flows;

public readonly record struct TcpEndpoint(string Address, ushort Port)
{
    public override string ToString()
    {
        var formattedAddress = Address.Contains(':', StringComparison.Ordinal)
            ? $"[{Address}]"
            : Address;
        return $"{formattedAddress}:{Port}";
    }
}

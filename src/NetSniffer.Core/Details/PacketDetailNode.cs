namespace NetSniffer.Core.Details;

public sealed record PacketDetailNode(
    string Name,
    string Value,
    IReadOnlyList<PacketDetailNode> Children)
{
    public PacketDetailNode(string name, string value)
        : this(name, value, [])
    {
    }

    public string DisplayText => string.IsNullOrWhiteSpace(Value) ? Name : $"{Name}: {Value}";
}


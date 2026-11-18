namespace NetSniffer.Core.Models;

/// <summary>
/// A UI-friendly snapshot of a capture device exposed by Npcap.
/// </summary>
public sealed record CaptureDeviceInfo(
    string Name,
    string Description,
    IReadOnlyList<string> Addresses,
    string MacAddress)
{
    public string DisplayName
    {
        get
        {
            var label = string.IsNullOrWhiteSpace(Description) ? Name : Description;
            return Addresses.Count == 0
                ? label
                : $"{label} ({string.Join(", ", Addresses)})";
        }
    }

    public override string ToString() => DisplayName;
}


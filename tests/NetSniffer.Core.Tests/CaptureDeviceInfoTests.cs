using NetSniffer.Core.Models;

namespace NetSniffer.Core.Tests;

public class CaptureDeviceInfoTests
{
    [Fact]
    public void DisplayNameUsesDescriptionAndAddressesWhenBothAreAvailable()
    {
        var device = new CaptureDeviceInfo(
            "npcap-device-id",
            "Ethernet Adapter",
            ["192.0.2.10", "2001:db8::10"],
            "001122334455");

        Assert.Equal(
            "Ethernet Adapter (192.0.2.10, 2001:db8::10)",
            device.DisplayName);
    }

    [Fact]
    public void DisplayNameFallsBackToDeviceNameWhenDescriptionIsEmpty()
    {
        var device = new CaptureDeviceInfo(
            "npcap-device-id",
            string.Empty,
            [],
            string.Empty);

        Assert.Equal("npcap-device-id", device.DisplayName);
    }
}


using NetSniffer.Core.Models;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetSniffer.Core.Services;

public sealed class SharpPcapDeviceCatalog : ICaptureDeviceCatalog
{
    public IReadOnlyList<CaptureDeviceInfo> GetDevices()
    {
        try
        {
            var devices = CaptureDeviceList.Instance;
            devices.Refresh();

            return devices
                .Select(CreateDeviceInfo)
                .OrderBy(device => device.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }
        catch (Exception exception) when (exception is DllNotFoundException or TypeInitializationException)
        {
            throw new InvalidOperationException(
                "无法访问 Npcap。请确认已经安装 Npcap，并重新启动应用程序。",
                exception);
        }
    }

    private static CaptureDeviceInfo CreateDeviceInfo(ILiveDevice device)
    {
        var addresses = device is LibPcapLiveDevice pcapDevice
            ? pcapDevice.Addresses
                .Select(address => address.Addr?.ipAddress)
                .Where(address => address is not null)
                .Select(address => address!.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];

        var macAddress = device.MacAddress?.ToString() ?? string.Empty;

        return new CaptureDeviceInfo(
            device.Name,
            device.Description ?? string.Empty,
            addresses,
            macAddress);
    }
}


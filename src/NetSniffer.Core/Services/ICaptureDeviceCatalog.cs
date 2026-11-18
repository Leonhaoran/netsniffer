using NetSniffer.Core.Models;

namespace NetSniffer.Core.Services;

public interface ICaptureDeviceCatalog
{
    IReadOnlyList<CaptureDeviceInfo> GetDevices();
}


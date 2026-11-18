using System.Text;
using NetSniffer.Core.Details;

namespace NetSniffer.Core.Tests;

public class HexDumpFormatterTests
{
    [Fact]
    public void FormatIncludesOffsetHexBytesAndAscii()
    {
        var data = Encoding.ASCII.GetBytes("Hello, NetSniffer!");

        var result = HexDumpFormatter.Format(data);

        Assert.Contains("00000000", result, StringComparison.Ordinal);
        Assert.Contains("48 65 6C 6C 6F", result, StringComparison.Ordinal);
        Assert.Contains("|Hello, NetSniffe|", result, StringComparison.Ordinal);
        Assert.Contains("00000010", result, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatReturnsEmptyTextForEmptyPacket()
    {
        Assert.Equal(string.Empty, HexDumpFormatter.Format([]));
    }
}


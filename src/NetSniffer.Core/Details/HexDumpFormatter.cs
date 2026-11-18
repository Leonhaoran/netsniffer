using System.Globalization;
using System.Text;

namespace NetSniffer.Core.Details;

public static class HexDumpFormatter
{
    public static string Format(byte[] data, int bytesPerLine = 16)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesPerLine, 1);

        if (data.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        for (var offset = 0; offset < data.Length; offset += bytesPerLine)
        {
            var lineLength = Math.Min(bytesPerLine, data.Length - offset);
            builder.Append(offset.ToString("X8", CultureInfo.InvariantCulture));
            builder.Append("  ");

            for (var index = 0; index < bytesPerLine; index++)
            {
                if (index < lineLength)
                {
                    builder.Append(data[offset + index].ToString("X2", CultureInfo.InvariantCulture));
                    builder.Append(' ');
                }
                else
                {
                    builder.Append("   ");
                }

                if (index == 7)
                {
                    builder.Append(' ');
                }
            }

            builder.Append(" |");
            for (var index = 0; index < lineLength; index++)
            {
                var value = data[offset + index];
                builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
            }

            builder.Append('|');

            if (offset + lineLength < data.Length)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}


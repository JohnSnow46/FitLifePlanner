using System.Text;

namespace FitLifePlanner.Api.Common;

public static class CsvExport
{
    public static string ToCsv(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', headers.Select(Escape)));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(Escape)));
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

namespace HAC.EFTree.Tests.Helpers;

using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

static class TestOutputHelperExtensions
{
    static readonly List<string> allColumns = [];
    public static void AddColumns(this ITestOutputHelper output, params object[] columns)
        => allColumns.AddRange(columns.Select(x => x.ToString() ?? string.Empty));

    public static void WriteColumns(this ITestOutputHelper output, string separator = "\t")
    {
        // Split into lines
        var lines = allColumns.Select(x => x.Replace("\r\n", "\n").Split('\n')).ToArray();

        // Find maximum number of lines among all columns
        var maxLines = lines.DefaultIfEmpty().Max(c => c?.Length ?? 0);

        // Determine widths of each column for padding
        var widths = lines.Select(c => c.Max(l => l.Length)).ToArray();

        // Print row by row
        for (var row = 0; row < maxLines; row++)
        {
            var parts = lines.Select((x, i) => (x.ElementAtOrDefault(row) ?? string.Empty).PadRight(widths[i]));
            output.WriteLine(string.Join(separator, parts));
        }
        allColumns.Clear();
    }
}

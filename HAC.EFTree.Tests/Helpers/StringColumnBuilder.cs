namespace HAC.EFTree.Tests.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Text;

class StringColumnBuilder
{
    readonly List<string> allColumns = [];

    public StringColumnBuilder AddColumns(params object[] columns)
    {
        allColumns.AddRange(columns.Select(x => x.ToString() ?? string.Empty));
        return this;
    }

    public string ToString(string separator = "\t")
    {
        StringBuilder builder = new();

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
            builder.AppendLine(string.Join(separator, parts));
        }
        return builder.ToString();
    }

    public override string ToString() => ToString("\t");
}

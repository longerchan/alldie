using System.Collections.Generic;
using System.Text;

namespace FrostShelter.DataTable
{
    /// <summary>
    /// 简易CSV解析器，支持引号转义。
    /// </summary>
    public static class CsvParser
    {
        public static List<TableRow> Parse(string csvContent)
        {
            var rows = new List<TableRow>();
            if (string.IsNullOrEmpty(csvContent)) return rows;

            var lines = SplitLines(csvContent);
            if (lines.Count < 2) return rows;

            // 第一行为表头
            var headers = ParseLine(lines[0]);

            for (int i = 1; i < lines.Count; i++)
            {
                var values = ParseLine(lines[i]);
                if (values.Count == 0 || (values.Count == 1 && string.IsNullOrWhiteSpace(values[0])))
                    continue;

                var row = new TableRow();
                for (int j = 0; j < headers.Count && j < values.Count; j++)
                {
                    row[headers[j]] = values[j];
                }
                rows.Add(row);
            }

            return rows;
        }

        private static List<string> SplitLines(string content)
        {
            var lines = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in content)
            {
                if (c == '"') inQuotes = !inQuotes;
                if (c == '\n' && !inQuotes)
                {
                    lines.Add(sb.ToString().Trim('\r'));
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0) lines.Add(sb.ToString().Trim('\r'));

            return lines;
        }

        private static List<string> ParseLine(string line)
        {
            var values = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            values.Add(sb.ToString());

            return values;
        }
    }
}

using System.Text;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Small dependency-free CSV parser. Handles quoted fields, embedded commas,
    /// escaped double quotes ("") and both CRLF and LF line endings.
    /// </summary>
    public static class CsvReader
    {
        public static List<string[]> Parse(string content)
        {
            var rows = new List<string[]>();
            var field = new StringBuilder();
            var record = new List<string>();
            var inQuotes = false;

            for (var i = 0; i < content.Length; i++)
            {
                var c = content[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }

                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        rows.Add(record.ToArray());
                        record = new List<string>();
                        break;
                    default:
                        field.Append(c);
                        break;
                }
            }

            if (field.Length > 0 || record.Count > 0)
            {
                record.Add(field.ToString());
                rows.Add(record.ToArray());
            }

            return rows.Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v))).ToList();
        }
    }
}

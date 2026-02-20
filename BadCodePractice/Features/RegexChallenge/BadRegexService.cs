using System.Text.RegularExpressions;

namespace BadCodePractice.Features.RegexChallenge;

public class BadRegexService : IRegexService
{
    public string Name => "Bad Regex (Inline compilation, string allocations)";

    public List<ParsedLogEntry> ParseLogEntries(IEnumerable<string> logLines)
    {
        var results = new List<ParsedLogEntry>();

        foreach (var line in logLines)
        {
            // ANTI-PATTERN 1: Creating Regex inline on every call
            // This causes the regex pattern to be parsed and compiled on EVERY iteration
            var timestampRegex = new Regex(@"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]");
            var levelRegex = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\]");
            var correlationRegex = new Regex(@"\[corr:([a-fA-F0-9\-]+)\]");

            var timestampMatch = timestampRegex.Match(line);
            var levelMatch = levelRegex.Match(line);
            var correlationMatch = correlationRegex.Match(line);

            if (!timestampMatch.Success || !levelMatch.Success)
            {
                continue;
            }

            // ANTI-PATTERN 2: Excessive Substring allocations
            // Using Substring multiple times creates new strings for each operation
            var timestamp = timestampMatch.Groups[1].Value;
            var level = levelMatch.Groups[1].Value;

            // ANTI-PATTERN 3: More Substring allocations to extract message
            var messageStart = line.IndexOf(']') + 1;
            var messageEnd = line.LastIndexOf('[');
            var message = messageEnd > messageStart
                ? line.Substring(messageStart, messageEnd - messageStart).Trim()
                : line.Substring(messageStart).Trim();

            // ANTI-PATTERN 4: String concatenation for correlation ID
            var correlationId = correlationMatch.Success
                ? "" + correlationMatch.Groups[1].Value
                : "" + null;

            // ANTI-PATTERN 5: Creating new ParsedLogEntry with unnecessary allocations
            results.Add(new ParsedLogEntry(
                timestamp,
                level,
                message.ToUpper(),  // ANTI-PATTERN 6: ToUpper() without CultureInfo allocates
                correlationId.Length > 0 ? correlationId : null
            ));
        }

        return results;
    }
}

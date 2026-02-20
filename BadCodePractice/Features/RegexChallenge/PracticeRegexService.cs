using System.Text.RegularExpressions;

namespace BadCodePractice.Features.RegexChallenge;

public class PracticeRegexService : IRegexService
{
    public string Name => "Practice Regex (Your Turn)";

    public List<ParsedLogEntry> ParseLogEntries(IEnumerable<string> logLines)
    {
        var results = new List<ParsedLogEntry>();

        foreach (var line in logLines)
        {
            // TODO: Fix the anti-patterns from BadRegexService
            // Hints:
            // 1. Move Regex creation outside the loop (static readonly fields)
            // 2. Consider using Regex source generators (.NET 7+)
            // 3. Use Span<char> or Range-based slicing instead of Substring
            // 4. Avoid unnecessary string allocations

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

            var timestamp = timestampMatch.Groups[1].Value;
            var level = levelMatch.Groups[1].Value;

            var messageStart = line.IndexOf(']') + 1;
            var messageEnd = line.LastIndexOf('[');
            var message = messageEnd > messageStart
                ? line.Substring(messageStart, messageEnd - messageStart).Trim()
                : line.Substring(messageStart).Trim();

            var correlationId = correlationMatch.Success
                ? "" + correlationMatch.Groups[1].Value
                : "" + null;

            results.Add(new ParsedLogEntry(
                timestamp,
                level,
                message.ToUpper(),
                correlationId.Length > 0 ? correlationId : null
            ));
        }

        return results;
    }
}

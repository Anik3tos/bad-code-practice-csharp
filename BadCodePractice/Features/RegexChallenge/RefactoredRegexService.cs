using System.Globalization;
using System.Text.RegularExpressions;

namespace BadCodePractice.Features.RegexChallenge;

public partial class RefactoredRegexService : IRegexService
{
    public string Name => "AI Refactored Regex (Static regex, minimal allocations)";

    // FIX 1: Use source-generated regex (.NET 7+) - compiled once at build time
    [GeneratedRegex(@"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]", RegexOptions.Compiled)]
    private static partial Regex TimestampRegex();

    [GeneratedRegex(@"\[(DEBUG|INFO|WARN|ERROR)\]", RegexOptions.Compiled)]
    private static partial Regex LevelRegex();

    [GeneratedRegex(@"\[corr:([a-fA-F0-9\-]+)\]", RegexOptions.Compiled)]
    private static partial Regex CorrelationRegex();

    public List<ParsedLogEntry> ParseLogEntries(IEnumerable<string> logLines)
    {
        // FIX 2: Pre-allocate list with reasonable capacity if we know approximate size
        // For IEnumerable we can't know, but if caller passes List<T>, we could
        var results = new List<ParsedLogEntry>();

        foreach (var line in logLines)
        {
            // FIX 3: Reuse static regex instances - no per-call compilation
            var timestampMatch = TimestampRegex().Match(line);
            var levelMatch = LevelRegex().Match(line);
            var correlationMatch = CorrelationRegex().Match(line);

            if (!timestampMatch.Success || !levelMatch.Success)
            {
                continue;
            }

            var timestamp = timestampMatch.Groups[1].Value;
            var level = levelMatch.Groups[1].Value;

            // FIX 4: Use Span<char> and Range to avoid Substring allocations
            var lineSpan = line.AsSpan();
            var firstBracket = lineSpan.IndexOf(']');
            var lastBracket = lineSpan.LastIndexOf('[');

            ReadOnlySpan<char> messageSpan;
            if (lastBracket > firstBracket)
            {
                messageSpan = lineSpan.Slice(firstBracket + 1, lastBracket - firstBracket - 1).Trim();
            }
            else
            {
                messageSpan = lineSpan.Slice(firstBracket + 1).Trim();
            }

            // FIX 5: Only allocate string when we actually need it (for the result)
            var message = messageSpan.ToString();

            // FIX 6: Avoid unnecessary string concatenation for null
            string? correlationId = correlationMatch.Success
                ? correlationMatch.Groups[1].Value
                : null;

            // FIX 7: Use invariant culture forToUpper if culture-independent comparison needed
            // Or skip ToUpper entirely if not needed
            results.Add(new ParsedLogEntry(
                timestamp,
                level,
                message,
                correlationId
            ));
        }

        return results;
    }
}

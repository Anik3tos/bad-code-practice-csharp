namespace BadCodePractice.Features.RegexChallenge;

public interface IRegexService
{
    string Name { get; }
    List<ParsedLogEntry> ParseLogEntries(IEnumerable<string> logLines);
}

public sealed record ParsedLogEntry(
    string Timestamp,
    string Level,
    string Message,
    string? CorrelationId);

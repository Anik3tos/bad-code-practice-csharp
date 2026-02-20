using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BadCodePractice.Features.LoggingChallenge;

public sealed class InMemoryLoggerStore
{
    public ConcurrentBag<LogEntry> Entries { get; } = new();

    public void Clear() => Entries.Clear();
}

public sealed record LogEntry(
    LogLevel LogLevel,
    string Message,
    bool HasCorrelationId,
    int MessageLength);

public sealed class InMemoryLoggerProvider(InMemoryLoggerStore store) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(store);
    }

    public void Dispose()
    {
    }
}

public sealed class InMemoryLogger(InMemoryLoggerStore store) : ILogger
{
    // A clumsy way to detect correlation in our simulated environment
    // where DI might inject different scope providers. For this lab,
    // we just check if it was appended to the actual string if structured logging didn't catch it.
    private readonly AsyncLocal<string?> _currentCorrelationId = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        if (state is IEnumerable<KeyValuePair<string, object>> dict)
        {
            var correlationId = dict.FirstOrDefault(k => k.Key == "CorrelationId").Value?.ToString();
            if (correlationId is null)
            {
                // Also check trace
                correlationId = dict.FirstOrDefault(k => k.Key.Contains("Trace")).Value?.ToString();
            }

            if (correlationId is not null)
            {
                _currentCorrelationId.Value = correlationId;
                return new ScopeTracker(this);
            }
        }

        return null;
    }

    public bool IsEnabled(LogLevel logLevel) => true; // Enable everything for the test

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        
        bool hasCorrelation = _currentCorrelationId.Value is not null || message.Contains("req-");

        store.Entries.Add(new LogEntry(
            logLevel,
            message,
            hasCorrelation,
            message.Length
        ));
    }

    private sealed class ScopeTracker(InMemoryLogger logger) : IDisposable
    {
        public void Dispose()
        {
            logger._currentCorrelationId.Value = null;
        }
    }
}

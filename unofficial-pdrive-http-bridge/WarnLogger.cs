using System;
using Microsoft.Extensions.Logging;

namespace unofficial_pdrive_http_bridge;

/// <summary>
/// Logger that only logs Warning or above.
/// </summary>
public sealed class WarnLogger(ILogger inner) : ILogger
{
    private readonly ILogger _inner = inner;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _inner.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel < LogLevel.Warning && logLevel > LogLevel.Debug)
            logLevel = LogLevel.Debug;

        return _inner.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel < LogLevel.Warning && logLevel > LogLevel.Debug)
            logLevel = LogLevel.Debug;

        _inner.Log(logLevel, eventId, state, exception, formatter);
    }
}

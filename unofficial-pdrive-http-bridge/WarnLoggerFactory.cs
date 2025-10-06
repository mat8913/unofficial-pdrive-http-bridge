using Microsoft.Extensions.Logging;

namespace unofficial_pdrive_http_bridge;

/// <summary>
/// Logger Factory that only logs Warning or above.
/// </summary>
public sealed class WarnLoggerFactory(ILoggerFactory inner) : ILoggerFactory
{
    private readonly ILoggerFactory _inner = inner;

    public void AddProvider(ILoggerProvider provider)
    {
        _inner.AddProvider(provider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new WarnLogger(_inner.CreateLogger(categoryName));
    }

    public void Dispose()
    {
    }
}

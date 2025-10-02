using Microsoft.Extensions.Logging;

namespace DoDo.Net.Internal;

/// <summary>
/// Simple console logger implementation for the text extraction service
/// </summary>
class ConsoleLogger : ILogger
{
    public ConsoleLogger()
    {
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new NoOpDisposable(); // Return a no-op disposable
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information; // Only log Information and above
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logLevelString = GetLogLevelString(logLevel);
        
        Console.WriteLine($"[{timestamp}] [{logLevelString}]: {message}");
        
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "NONE",
            _ => logLevel.ToString().ToUpperInvariant()
        };
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
            // No-op
        }
    }
}

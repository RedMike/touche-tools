using Microsoft.Extensions.Logging;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App;

public class LoggerProvider : ILoggerProvider
{
    private readonly LogLevel _minLevel;
    private readonly LogData _data;

    public LoggerProvider(LogLevel minLevel, LogData data)
    {
        _minLevel = minLevel;
        _data = data;
    }

    public void Dispose()
    {
        
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new InAppLogger(_minLevel, _data);
    }
}

public class InAppLogger : ILogger
{
    private readonly LogLevel _minLevel;
    private readonly LogData _data;

    public InAppLogger(LogLevel minLevel, LogData data)
    {
        _minLevel = minLevel;
        _data = data;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Information)
        {
            _data.Info(formatter(state, exception));
        }

        if (logLevel >= LogLevel.Warning)
        {
            _data.Error(formatter(state, exception));
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLevel;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}
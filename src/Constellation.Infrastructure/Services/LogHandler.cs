namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Services;
using System.Collections.Generic;

internal sealed class LogHandler<T> : ILogHandler<T> where T : class
{
    private readonly List<string> _logHistory = new();
    private readonly ILogger _logger;

    public LogHandler(ILogger logger)
    {
        _logger = logger.ForContext<T>();
    }

    public void Log(LogSeverity severity, string message)
    {
        _logHistory.Add(message);

        switch (severity)
        {
            case LogSeverity.Information:
                _logger.Information(message);
                break;
            case LogSeverity.Warning:
                _logger.Warning(message);
                break;
            case LogSeverity.Error:
                _logger.Error(message);
                break;
            case LogSeverity.Critical:
                _logger.Fatal(message);
                break;
            default:
                _logger.Information(message);
                break;
        }
    }

    public List<string> GetLogHistory() => _logHistory;
}

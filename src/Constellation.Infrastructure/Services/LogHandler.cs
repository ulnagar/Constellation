namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using System.Collections.Generic;

public partial class LogHandler<T> : ILogHandler<T>, IScopedService where T : class
{
    private ICollection<string> _logHistory;
    private readonly ILogger _logger;

    public LogHandler(ILogger logger)
    {
        _logHistory = new List<string>();
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

    public ICollection<string> GetLogHistory()
    {
        return _logHistory;
    }
}

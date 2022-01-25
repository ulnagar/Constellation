using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Services
{
    public partial class LogHandler<T> : ILogHandler<T>, IScopedService where T : class
    {
        private ICollection<string> _logHistory;
        private readonly ILogger<T> _logger;

        public LogHandler(ILogger<T> logger)
        {
            _logHistory = new List<string>();
            _logger = logger;
        }

        public void Log(LogSeverity severity, string message)
        {
            _logHistory.Add(message);

            switch (severity)
            {
                case LogSeverity.Information:
                    _logger.LogInformation(message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(message);
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(message);
                    break;
            }

            _logger.LogInformation(message);
        }

        public ICollection<string> GetLogHistory()
        {
            return _logHistory;
        }
    }
}

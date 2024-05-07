namespace Constellation.Application.Interfaces.Services;

using Constellation.Infrastructure.Services;
using System.Collections.Generic;

public interface ILogHandler<T>
{
    void Log(LogSeverity severity, string message);
    List<string> GetLogHistory();
}
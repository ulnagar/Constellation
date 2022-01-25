using Constellation.Infrastructure.Services;
using System.Collections.Generic;

namespace Constellation.Application.Interfaces.Services
{
    public interface ILogHandler<T>
    {
        void Log(LogSeverity severity, string message);
        ICollection<string> GetLogHistory();
    }
}

using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IClassMonitorJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

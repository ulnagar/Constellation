using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ISentralReportSyncJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

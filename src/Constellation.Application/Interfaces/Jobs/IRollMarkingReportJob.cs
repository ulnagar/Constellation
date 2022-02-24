using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IRollMarkingReportJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

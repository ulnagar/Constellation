using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ITrackItSyncJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

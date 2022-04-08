using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ISentralPhotoSyncJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

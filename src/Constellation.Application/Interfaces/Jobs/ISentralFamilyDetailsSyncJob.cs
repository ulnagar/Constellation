using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ISentralFamilyDetailsSyncJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

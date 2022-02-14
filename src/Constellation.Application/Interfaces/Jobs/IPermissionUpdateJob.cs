using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IPermissionUpdateJob : IHangfireJob
    {
        Task StartJob();
    }
}

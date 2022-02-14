using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IPermissionUpdateJob
    {
        Task StartJob();
    }
}

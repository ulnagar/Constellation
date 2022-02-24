using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IUserManagerJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

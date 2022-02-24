using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface ISchoolRegisterJob : IHangfireJob
    {
        Task StartJob(bool automated);
    }
}

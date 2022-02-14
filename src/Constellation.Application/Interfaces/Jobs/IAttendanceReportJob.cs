using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IAttendanceReportJob : IHangfireJob
    {
        Task StartJob();
    }
}

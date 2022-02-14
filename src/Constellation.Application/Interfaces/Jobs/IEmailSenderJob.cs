using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IEmailSenderJob : IHangfireJob
    {
        Task StartJob();
    }
}

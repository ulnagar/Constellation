using MimeKit;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface IEmailGateway
    {
        Task Send(MailMessage message);
        Task Send(MimeMessage message);
    }
}

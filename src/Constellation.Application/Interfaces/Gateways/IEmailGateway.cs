using MimeKit;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface IEmailGateway
    {
        Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments);
        Task Send(MailMessage message);
    }
}

namespace Constellation.Application.Interfaces.Gateways;

using Core.Models.Messaging.Email;
using Core.Shared;
using MimeKit;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailGateway
{
    Task<Result<string>> Send(EmailMessage message, List<Attachment>?  attachments = null, string? calendarInfo = null, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
}
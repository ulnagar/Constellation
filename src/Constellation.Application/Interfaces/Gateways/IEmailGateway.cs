namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Core.ValueObjects;
using Core.Shared;
using MimeKit;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailGateway
{
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, EmailRecipient fromRecipient, string subject, string body, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, string fromAddress, string subject, string body, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, EmailRecipient fromAddress, string subject, string body, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, List<EmailRecipient> bccRecipients, string fromAddress, string subject, string body, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, List<EmailRecipient> bccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
    Task<Result<MimeMessage>> Send(List<EmailRecipient> toRecipients, EmailRecipient fromAddress, string subject, string body, ICollection<Attachment> attachments, MessagePriority priority = MessagePriority.Normal, CancellationToken cancellationToken = default);
}
namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Core.ValueObjects;
using MimeKit;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailGateway
{
    // Using EmailRecipient value objects
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, List<EmailRecipient> bccRecipients, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(List<EmailRecipient> toRecipients, List<EmailRecipient> ccRecipients, List<EmailRecipient> bccRecipients, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);

    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, string calendarInfo, CancellationToken cancellationToken = default);
    Task<MimeMessage> Send(IDictionary<string, string> toAddresses, IDictionary<string, string> ccAddresses, IDictionary<string, string> bccAddresses, string fromAddress, string subject, string body, ICollection<Attachment> attachments, string calendarInfo, CancellationToken cancellationToken = default);
}

namespace Constellation.Core.Models.Messaging.Email.Repositories;

using Identifiers;
using ValueObjects;

public interface IEmailRepository
{
    Task<EmailMessage?> GetById(EmailId id, CancellationToken cancellationToken = default);
    Task<List<EmailMessage>> GetByRecipient(EmailAddress email, CancellationToken cancellationToken = default);
    Task<List<EmailMessage>> GetRecent(int count, CancellationToken cancellationToken = default);

    void Insert(EmailMessage message);
}

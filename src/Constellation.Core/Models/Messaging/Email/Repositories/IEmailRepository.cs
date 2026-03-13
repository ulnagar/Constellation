namespace Constellation.Core.Models.Messaging.Email.Repositories;

using Constellation.Core.Models.Messaging.Sms;
using Identifiers;

public interface IEmailRepository
{
    Task<EmailMessage?> GetById(EmailId id, CancellationToken cancellationToken = default);

    void Insert(EmailMessage message);
}

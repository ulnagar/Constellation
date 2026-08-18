namespace Constellation.Application.Domains.Messaging.Tracking.Repositories;

using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using System.Collections.Generic;

public interface IEmailTrackingRepository
{
    Task<List<EmailTrackingEvent>> GetTrackingEventsByEmailId(EmailId emailId, CancellationToken cancellationToken = default);
}

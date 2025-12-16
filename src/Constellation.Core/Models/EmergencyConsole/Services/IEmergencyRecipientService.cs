namespace Constellation.Core.Models.EmergencyConsole.Services;

using Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValueObjects;

public interface IEmergencyRecipientService
{
    Task<List<EmailRecipient>> GetSelectedEmailRecipientsFromGroup(RecipientGroup group, CancellationToken cancellationToken = default);
}

namespace Constellation.Core.Models.Messaging.EmergencyConsole.Services;

using Enums;
using ValueObjects;

public interface IEmergencyRecipientService
{
    Task<List<AlertRecipient>> GetSelectedRecipientsFromGroup(RecipientGroup group, CancellationToken cancellationToken = default);
}
namespace Constellation.Core.Models.Messaging.EmergencyConsole.Services;

using Application.Domains.Contacts.Models;
using Enums;

public interface IEmergencyRecipientService
{
    Task<List<ContactResponse>> GetSelectedRecipientsFromGroup(RecipientGroup group, CancellationToken cancellationToken = default);
}
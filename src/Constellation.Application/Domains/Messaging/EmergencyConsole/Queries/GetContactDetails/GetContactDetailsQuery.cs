namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetContactDetails;

using Abstractions.Messaging;

public sealed record GetContactDetailsQuery()
    : IQuery<List<ContactDetail>>;
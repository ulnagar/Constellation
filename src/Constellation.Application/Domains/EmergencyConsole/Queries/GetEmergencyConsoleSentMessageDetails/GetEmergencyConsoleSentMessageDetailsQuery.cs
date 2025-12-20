namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageDetails;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.EmergencyConsole.Identifiers;

public sealed record GetEmergencyConsoleSentMessageDetailsQuery(
    EventId EventId)
    : IQuery<SentMessageDetail>;
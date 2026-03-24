namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventDetails;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record GetEmergencyConsoleMessageEventDetailsQuery(
    EventId EventId)
    : IQuery<MessageEventDetail>;
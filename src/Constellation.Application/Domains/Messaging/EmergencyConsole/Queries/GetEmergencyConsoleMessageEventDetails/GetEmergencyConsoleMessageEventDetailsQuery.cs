namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventDetails;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record GetEmergencyConsoleMessageEventDetailsQuery(
    EventId EventId)
    : IQuery<MessageEventDetail>;
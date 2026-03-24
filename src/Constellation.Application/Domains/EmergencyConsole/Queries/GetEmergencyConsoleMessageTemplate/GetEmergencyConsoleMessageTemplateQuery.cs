namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record GetEmergencyConsoleMessageTemplateQuery(
    TemplateId Id)
    : IQuery<MessageTemplate>;

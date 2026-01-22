namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Identifiers;

public sealed record GetEmergencyConsoleMessageTemplateQuery(
    TemplateId Id)
    : IQuery<MessageTemplate>;

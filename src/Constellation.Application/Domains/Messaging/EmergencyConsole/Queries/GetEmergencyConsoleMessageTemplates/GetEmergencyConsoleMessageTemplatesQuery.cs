namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplates;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;

public sealed record GetEmergencyConsoleMessageTemplatesQuery()
    : IQuery<List<MessageTemplate>>;

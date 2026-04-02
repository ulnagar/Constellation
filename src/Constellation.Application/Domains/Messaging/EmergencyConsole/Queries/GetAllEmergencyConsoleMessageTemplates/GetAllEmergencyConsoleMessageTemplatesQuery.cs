namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetAllEmergencyConsoleMessageTemplates;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;

public sealed record GetAllEmergencyConsoleMessageTemplatesQuery()
    : IQuery<List<MessageTemplate>>;

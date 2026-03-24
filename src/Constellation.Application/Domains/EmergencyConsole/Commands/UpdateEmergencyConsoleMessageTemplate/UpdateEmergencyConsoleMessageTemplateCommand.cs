namespace Constellation.Application.Domains.EmergencyConsole.Commands.UpdateEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record UpdateEmergencyConsoleMessageTemplateCommand(
    TemplateId Id,
    string Name,
    string Template)
    : ICommand;
namespace Constellation.Application.Domains.EmergencyConsole.Commands.DeleteEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record DeleteEmergencyConsoleMessageTemplateCommand(
    TemplateId Id)
    : ICommand;

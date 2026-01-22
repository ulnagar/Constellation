namespace Constellation.Application.Domains.EmergencyConsole.Commands.DeleteEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Identifiers;

public sealed record DeleteEmergencyConsoleMessageTemplateCommand(
    TemplateId Id)
    : ICommand;

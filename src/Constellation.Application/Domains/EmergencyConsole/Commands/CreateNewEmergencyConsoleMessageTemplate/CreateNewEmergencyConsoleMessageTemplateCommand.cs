namespace Constellation.Application.Domains.EmergencyConsole.Commands.CreateNewEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Enums;

public sealed record CreateNewEmergencyConsoleMessageTemplateCommand(
    MessageType Type,
    string Name,
    string Template)
    : ICommand;

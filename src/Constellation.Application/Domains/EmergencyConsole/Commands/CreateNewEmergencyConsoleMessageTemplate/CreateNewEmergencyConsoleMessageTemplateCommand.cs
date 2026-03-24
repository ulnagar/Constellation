namespace Constellation.Application.Domains.EmergencyConsole.Commands.CreateNewEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Enums;
using Core.Models.Messaging.Enums;

public sealed record CreateNewEmergencyConsoleMessageTemplateCommand(
    MessageType Type,
    string Name,
    string Template)
    : ICommand;

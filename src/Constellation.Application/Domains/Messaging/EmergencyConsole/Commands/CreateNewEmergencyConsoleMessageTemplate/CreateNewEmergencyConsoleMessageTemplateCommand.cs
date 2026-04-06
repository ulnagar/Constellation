namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Commands.CreateNewEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Models.Messaging.Enums;

public sealed record CreateNewEmergencyConsoleMessageTemplateCommand(
    MessageType Type,
    string Name,
    string Template)
    : ICommand;

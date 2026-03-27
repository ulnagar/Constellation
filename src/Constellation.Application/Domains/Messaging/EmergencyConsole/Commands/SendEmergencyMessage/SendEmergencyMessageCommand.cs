namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Commands.SendEmergencyMessage;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole.Enums;
using Core.Models.Messaging.Enums;
using Core.ValueObjects;

public sealed record SendEmergencyMessageCommand(
    List<RecipientGroup> RecipientGroups,
    List<AlertRecipient> Recipients,
    MessageType Type,
    string Message)
    : ICommand;

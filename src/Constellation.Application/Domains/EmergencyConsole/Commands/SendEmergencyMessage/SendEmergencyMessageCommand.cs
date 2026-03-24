namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessage;

using Abstractions.Messaging;
using Constellation.Core.ValueObjects;
using Core.Models.Messaging.EmergencyConsole.Enums;
using Core.Models.Messaging.Enums;
using System.Collections.Generic;

public sealed record SendEmergencyMessageCommand(
    List<RecipientGroup> RecipientGroups,
    List<AlertRecipient> Recipients,
    MessageType Type,
    string Message)
    : ICommand;

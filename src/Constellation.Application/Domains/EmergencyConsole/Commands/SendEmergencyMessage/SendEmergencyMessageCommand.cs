namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessage;

using Abstractions.Messaging;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record SendEmergencyMessageCommand(
    List<RecipientGroup> RecipientGroups,
    List<AlertRecipient> Recipients,
    MessageType Type,
    string Message)
    : ICommand;

namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsSMS;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Enums;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record SendEmergencyMessageAsSMSCommand(
    List<RecipientGroup> RecipientGroups,
    List<AlertRecipient> Recipients,
    string Message)
    : ICommand;

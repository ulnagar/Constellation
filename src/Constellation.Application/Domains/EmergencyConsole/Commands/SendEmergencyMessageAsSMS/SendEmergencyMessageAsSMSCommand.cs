namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsSMS;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Enums;
using System.Collections.Generic;

public sealed record SendEmergencyMessageAsSMSCommand(
    List<RecipientGroup> RecipientGroups,
    string Recipients,
    string Message)
    : ICommand;

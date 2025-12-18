namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsEmail;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Enums;
using Core.ValueObjects;

public sealed record SendEmergencyMessageAsEmailCommand(
    List<RecipientGroup> RecipientGroups,
    List<AlertRecipient> Recipients,
    string Message)
    : ICommand;

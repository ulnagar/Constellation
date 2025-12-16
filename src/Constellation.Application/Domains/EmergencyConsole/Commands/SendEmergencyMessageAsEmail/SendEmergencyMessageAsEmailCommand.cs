namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsEmail;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole.Enums;

public sealed record SendEmergencyMessageAsEmailCommand(
    List<RecipientGroup> RecipientGroups,
    string Recipients,
    string Message)
    : ICommand;

namespace Constellation.Application.MissedWork.CreateClassworkNotificationResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CreateClassworkNotificationResponseCommand(
    ClassworkNotificationId NotificationId,
    string TeacherEmailAddress,
    string Description)
    : ICommand;
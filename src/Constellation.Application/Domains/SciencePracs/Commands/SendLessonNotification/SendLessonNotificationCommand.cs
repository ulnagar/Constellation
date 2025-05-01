namespace Constellation.Application.Domains.SciencePracs.Commands.SendLessonNotification;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record SendLessonNotificationCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    bool ShouldIncrementCount)
    : ICommand;

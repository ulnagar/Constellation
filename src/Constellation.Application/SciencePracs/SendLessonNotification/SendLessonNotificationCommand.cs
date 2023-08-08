namespace Constellation.Application.SciencePracs.SendLessonNotification;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record SendLessonNotificationCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    bool ShouldIncrementCount)
    : ICommand;

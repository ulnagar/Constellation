namespace Constellation.Application.SciencePracs.ReinstateLessonRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record ReinstateLessonRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : ICommand;
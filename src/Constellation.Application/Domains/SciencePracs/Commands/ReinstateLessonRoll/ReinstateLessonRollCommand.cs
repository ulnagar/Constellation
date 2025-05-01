namespace Constellation.Application.Domains.SciencePracs.Commands.ReinstateLessonRoll;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record ReinstateLessonRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : ICommand;
namespace Constellation.Application.SciencePracs.CancelLessonRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CancelLessonRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    string Comment)
    : ICommand;
namespace Constellation.Application.Domains.SciencePracs.Commands.CancelLessonRoll;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CancelLessonRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    string Comment)
    : ICommand;
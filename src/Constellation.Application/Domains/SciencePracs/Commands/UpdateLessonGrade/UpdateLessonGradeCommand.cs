namespace Constellation.Application.Domains.SciencePracs.Commands.UpdateLessonGrade;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record UpdateLessonGradeCommand(
    SciencePracLessonId LessonId)
    : ICommand;
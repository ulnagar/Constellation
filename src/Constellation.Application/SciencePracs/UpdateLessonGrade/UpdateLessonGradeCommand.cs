namespace Constellation.Application.SciencePracs.UpdateLessonGrade;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record UpdateLessonGradeCommand(
    SciencePracLessonId LessonId)
    : ICommand;
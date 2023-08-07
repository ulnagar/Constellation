namespace Constellation.Application.SciencePracs.CancelLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CancelLessonCommand(
    SciencePracLessonId LessonId)
    : ICommand;

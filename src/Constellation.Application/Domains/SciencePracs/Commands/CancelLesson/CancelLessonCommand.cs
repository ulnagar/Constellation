namespace Constellation.Application.Domains.SciencePracs.Commands.CancelLesson;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CancelLessonCommand(
    SciencePracLessonId LessonId)
    : ICommand;

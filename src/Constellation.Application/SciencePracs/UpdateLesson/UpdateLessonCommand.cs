namespace Constellation.Application.SciencePracs.UpdateLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record UpdateLessonCommand(
    SciencePracLessonId LessonId,
    string Name,
    DateOnly DueDate)
    : ICommand;

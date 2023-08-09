namespace Constellation.Application.SciencePracs.CreateLesson;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record CreateLessonCommand(
    string Name,
    DateOnly DueDate,
    int CourseId,
    bool DoNotGenerateRolls)
    : ICommand;
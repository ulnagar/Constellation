namespace Constellation.Application.SciencePracs.CreateLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record CreateLessonCommand(
    string Name,
    DateOnly DueDate,
    CourseId CourseId,
    bool DoNotGenerateRolls)
    : ICommand;
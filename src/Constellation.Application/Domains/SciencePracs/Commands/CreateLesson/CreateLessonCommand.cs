namespace Constellation.Application.Domains.SciencePracs.Commands.CreateLesson;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using System;

public sealed record CreateLessonCommand(
    string Name,
    DateOnly DueDate,
    CourseId CourseId,
    bool DoNotGenerateRolls)
    : ICommand;
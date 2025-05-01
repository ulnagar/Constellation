namespace Constellation.Application.Domains.SciencePracs.Commands.UpdateLesson;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using System;

public sealed record UpdateLessonCommand(
    SciencePracLessonId LessonId,
    string Name,
    DateOnly DueDate)
    : ICommand;

namespace Constellation.Application.Domains.SciencePracs.Commands.SubmitRoll;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SubmitRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    DateTime LessonDate,
    string Comment,
    List<StudentId> PresentStudents,
    List<StudentId> AbsentStudents)
    : ICommand;

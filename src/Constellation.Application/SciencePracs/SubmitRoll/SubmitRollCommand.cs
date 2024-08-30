namespace Constellation.Application.SciencePracs.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
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

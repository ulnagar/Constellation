namespace Constellation.Application.SciencePracs.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SubmitRollCommand(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    DateOnly LessonDate,
    string Comment,
    List<string> PresentStudents,
    List<string> AbsentStudents)
    : ICommand;

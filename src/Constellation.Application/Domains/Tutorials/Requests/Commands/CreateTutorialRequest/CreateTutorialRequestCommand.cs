namespace Constellation.Application.Domains.Tutorials.Requests.Commands.CreateTutorialRequest;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Enums;
using System.Collections.Generic;

public sealed record CreateTutorialRequestCommand(
    StudentId StudentId,
    TutorialType TutorialType,
    CourseId CourseId,
    List<PeriodId> PeriodIds,
    string Justification)
    : ICommand;
namespace Constellation.Application.Absences.SendMissedWorkEmailToStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record SendMissedWorkEmailToStudentCommand(
    Guid JobId,
    StudentId StudentId,
    OfferingId OfferingId,
    DateOnly AbsenceDate)
    : ICommand;
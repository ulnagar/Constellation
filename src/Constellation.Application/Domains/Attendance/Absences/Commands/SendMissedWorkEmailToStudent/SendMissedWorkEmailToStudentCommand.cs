namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendMissedWorkEmailToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using System;

public sealed record SendMissedWorkEmailToStudentCommand(
    Guid JobId,
    StudentId StudentId,
    OfferingId OfferingId,
    DateOnly AbsenceDate)
    : ICommand;
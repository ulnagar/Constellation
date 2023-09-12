namespace Constellation.Application.Absences.SendMissedWorkEmailToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record SendMissedWorkEmailToStudentCommand(
    Guid JobId,
    string StudentId,
    OfferingId OfferingId,
    DateOnly AbsenceDate)
    : ICommand;
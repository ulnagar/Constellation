namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAbsencesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record ExportAbsencesReportCommand(
    List<OfferingId> OfferingCodes,
    List<CourseId> CourseIds,
    List<Grade> Grades,
    List<SchoolCode> SchoolCodes,
    List<StudentId> StudentIds)
    : ICommand<FileDto>;
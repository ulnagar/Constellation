namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollDetails;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record LessonRollDetailsResponse(
    SciencePracLessonId LessonId,
    string Name,
    DateOnly DueDate,
    string SchoolCode,
    string SchoolName,
    LessonRollDetailsResponse.Contact ContactDetails,
    DateOnly? DateDelivered,
    DateTime? DateSubmitted,
    string Comment,
    LessonStatus Status,
    int NotificationCount,
    List<LessonRollDetailsResponse.AttendanceRecord> Attendance)
{
    public sealed record Contact(
        SchoolContactId ContactId,
        Name ContactName,
        EmailAddress ContactEmail);

    public sealed record AttendanceRecord(
        SciencePracAttendanceId AttendanceId,
        StudentId StudentId,
        StudentReferenceNumber StudentReferenceNumber,
        Name StudentName,
        bool Present);
}
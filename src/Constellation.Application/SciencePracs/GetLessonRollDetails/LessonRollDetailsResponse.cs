namespace Constellation.Application.SciencePracs.GetLessonRollDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
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
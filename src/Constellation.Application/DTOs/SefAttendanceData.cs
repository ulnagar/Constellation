namespace Constellation.Application.DTOs;

using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record SefAttendanceData(
    string StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    Name Student,
    Grade Grade,
    List<string> EnrolledClasses,
    int EnrolledDays,
    int TotalAbsentDays,
    int TotalPresentDays,
    decimal TotalPercentage,
    int JustifiedAbsentDays,
    int UnjustifiedAbsentDays,
    int UnjustifiedPresentDays,
    decimal UnjustifiedPercentage,
    List<string> AbsentDateData,
    List<string> JustifiedAbsentDateData);
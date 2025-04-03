namespace Constellation.Application.DTOs;

using Constellation.Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record SefAttendanceData(
    StudentId StudentId,
    Name Student,
    int EnrolledDays,
    int AbsentDays,
    int JustifiedDays,
    int PresentDays,
    decimal Percentage,
    List<DateOnly> AbsentDates);
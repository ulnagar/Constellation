﻿namespace Constellation.Core.Models.Attendance.Repositories;

using Constellation.Core.Models.Students.Identifiers;
using Core.Enums;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAttendanceRepository
{
    Task<AttendanceValue?> GetById(AttendanceValueId id, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllRecent(CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForStudent(int year, StudentId studentId, CancellationToken cancellationToken = default);
    Task<AttendanceValue?> GetLatestForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForDate(DateOnly selectedDate, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForStudentAndDate(StudentId studentId, DateOnly selectedDate, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForGrade(int year, Grade selectedGrade, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForGradeAndDate(Grade selectedGrade, DateOnly selectedDate, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetForReportWithTitle(string periodLabel, CancellationToken cancellationToken = default);
    Task<List<DateOnly>> GetPeriodEndDates(int year, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetForStudentBetweenDates(StudentId studentId, DateOnly earlierEndDate, DateOnly laterEndDate, CancellationToken cancellationToken = default);
    Task<List<string>> GetPeriodNames(int year, CancellationToken cancellationToken = default);
    void Insert(AttendanceValue item);
    void Insert(List<AttendanceValue> items);
}
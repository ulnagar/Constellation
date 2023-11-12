namespace Constellation.Core.Models.Attendance.Repositories;

using Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAttendanceRepository
{
    Task<List<AttendanceValue>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllRecent(CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForStudent(int year, string studentId, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForDate(DateOnly selectedDate, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForStudentAndDate(string studentId, DateOnly selectedDate, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForGrade(int year, Grade selectedGrade, CancellationToken cancellationToken = default);
    Task<List<AttendanceValue>> GetAllForGradeAndDate(Grade selectedGrade, DateOnly selectedDate, CancellationToken cancellationToken = default);

    void Insert(AttendanceValue item);
    void Insert(List<AttendanceValue> items);
}

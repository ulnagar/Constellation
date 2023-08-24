namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IOfferingRepository
{
    Task<Offering> GetById(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Offering>> GetActiveForTeacher(string StaffId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Offering>> GetActiveByCourseId(int courseId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetActiveByGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetByCourseId(int courseId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(string studentId, DateTime AbsenceDate, int DayNumber, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(string studentId, DateOnly AbsenceDate, int DayNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current Course Offerings that a student is enrolled in
    /// </summary>
    /// <param name="studentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Offering>> GetByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<Offering> GetFromYearAndName(int year, string name, CancellationToken cancellationToken = default);
}
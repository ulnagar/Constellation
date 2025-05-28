namespace Constellation.Core.Models.Offerings.Repositories;

using Canvas.Models;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Enums;
using Identifiers;
using Offerings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Timetables.Enums;
using Timetables.ValueObjects;

public interface IOfferingRepository
{
    Task<Offering?> GetById(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAllFuture(CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAllInactive(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current Course Offerings where the provided StaffId is listed as Classroom Teacher (only)
    /// </summary>
    /// <param name="StaffId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Offering>> GetActiveForTeacher(string StaffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current Course Offerings where the provided StaffId is listed as any type of Teacher
    /// </summary>
    /// <param name="staffId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Offering>> GetAllTypesActiveForTeacher(string staffId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Offering>> GetActiveByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetActiveByGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(StudentId studentId, DateTime AbsenceDate, PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(StudentId studentId, DateOnly AbsenceDate, PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current Course Offerings that a student is enrolled in
    /// </summary>
    /// <param name="studentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Offering>> GetByStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<Offering?> GetFromYearAndName(int year, string name, CancellationToken cancellationToken = default);

    void Insert(Offering offering);
    void Remove(Resource resource);

    Task<List<Timetable>> GetTimetableByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetWithLinkedTeamResource(string teamName, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetWithLinkedCanvasResource(CanvasCourseCode courseCode, CancellationToken cancellationToken = default);

    Task<List<Offering>> GetOfferingsFromSameGroup(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetListFromIds(List<OfferingId> offeringIds, CancellationToken cancellationToken = default);
}
namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface ICourseOfferingRepository
{
    Task<Offering?> GetById(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Offering>> GetAllActive(CancellationToken cancellationToken = default);
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

    Offering WithDetails(OfferingId id);
    Offering WithFilter(Expression<Func<Offering, bool>> predicate);
    Task<Offering> GetForExistCheck(OfferingId id);
    ICollection<Offering> All();
    ICollection<Offering> AllWithFilter(Expression<Func<Offering, bool>> predicate);
    ICollection<Offering> AllCurrentOfferings();
    ICollection<Offering> AllFutureOfferings();
    ICollection<Offering> AllPastOfferings();
    ICollection<Offering> AllFromFaculty(Faculty faculty);
    ICollection<Offering> AllFromGrade(Grade grade);
    ICollection<Offering> AllForStudent(string id);
    ICollection<Offering> AllCurrentAndFutureForStudent(string id);
    ICollection<Offering> AllCurrentForStudent(string id);
    ICollection<Offering> AllForTeacher(string id);
    ICollection<Staff> AllClassTeachers(OfferingId id);
    Task<ICollection<Offering>> ForSelectionAsync();
    Task<ICollection<Offering>> FromGradeForBulkEnrolAsync(Grade grade);
    Task<Offering> ForEnrolmentAsync(OfferingId id);
    Task<ICollection<Offering>> AllForTeacherAsync(string id);
    Task<ICollection<Offering>> ForListAsync(Expression<Func<Offering, bool>> predicate);
    Task<Offering> ForDetailDisplayAsync(OfferingId id);
    Task<Offering> ForEditAsync(OfferingId id);
    Task<Offering> ForSessionEditAsync(OfferingId id);
    Task<Offering> ForRollCreationAsync(OfferingId id);
    Task<Offering> ForCoverCreationAsync(OfferingId id);
    Task<ICollection<Staff>> AllTeachersForCoverCreationAsync(OfferingId id);
    Task<Offering> ForSessionCreationAsync(OfferingId id);
    Task<bool> AnyWithId(OfferingId id);
    Task<ICollection<Offering>> ForTeacherAndDates(string staffId, ICollection<int> dates);
    Task<Offering> GetFromYearAndName(int year, string name);
}
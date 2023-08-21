namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface ICourseOfferingRepository
{
    Task<Offering?> GetById(int offeringId, CancellationToken cancellationToken = default);
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

    Offering WithDetails(int id);
    Offering WithFilter(Expression<Func<Offering, bool>> predicate);
    Task<Offering> GetForExistCheck(int id);
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
    ICollection<Staff> AllClassTeachers(int id);
    Task<ICollection<Offering>> ForSelectionAsync();
    Task<ICollection<Offering>> FromGradeForBulkEnrolAsync(Grade grade);
    Task<Offering> ForEnrolmentAsync(int id);
    Task<ICollection<Offering>> AllForTeacherAsync(string id);
    Task<ICollection<Offering>> ForListAsync(Expression<Func<Offering, bool>> predicate);
    Task<Offering> ForDetailDisplayAsync(int id);
    Task<Offering> ForEditAsync(int id);
    Task<Offering> ForSessionEditAsync(int id);
    Task<Offering> ForRollCreationAsync(int id);
    Task<Offering> ForCoverCreationAsync(int id);
    Task<ICollection<Staff>> AllTeachersForCoverCreationAsync(int id);
    Task<Offering> ForSessionCreationAsync(int id);
    Task<bool> AnyWithId(int id);
    Task<ICollection<Offering>> ForTeacherAndDates(string staffId, ICollection<int> dates);
    Task<Offering> GetFromYearAndName(int year, string name);
}
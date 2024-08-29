namespace Constellation.Core.Models.Students.Repositories;

using Constellation.Core.Enums;
using Identifiers;
using Offerings.Identifiers;
using Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface IStudentRepository
{
    Task<List<Student>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Student>> GetAllWithSchool(CancellationToken cancellationToken = default);
    Task<Student?> GetById(StudentId studentId, CancellationToken cancellationToken = default);
    Task<Student?> GetBySRN(StudentReferenceNumber studentReferenceNumber, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudents(CancellationToken cancellationToken = default);
    Task<List<Student>> GetInactiveStudents(CancellationToken cancellationToken = default);
    Task<List<Student>> GetListFromIds(List<StudentId> studentIds, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithFamilyMemberships(CancellationToken cancellationToken = default);
    Task<bool> IsValidStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetFilteredStudents(List<OfferingId> offeringIds, List<Grade> grades, List<string> schoolCodes, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsFromSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentFromGrade(Grade grade, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a current student with a specified email address. Only returns current students.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Student?> GetCurrentByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get any student with a specified email address. Can return deleted students.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Student?> GetAnyByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);

    Task<int> GetCountCurrentStudentsWithPartialAbsenceScanDisabled(CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStudentsWithWholeAbsenceScanDisabled(CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStudentsWithoutSentralId(CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithoutSentralId(CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStudentsWithAwardOverages(CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStudentsWithPendingAwards(CancellationToken cancellationToken = default);

    Task<Student?> GetForExistCheck(StudentId id);
    Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate);
    Task<Student?> ForEditAsync(StudentId studentId);
    Task<Student?> ForBulkUnenrolAsync(StudentId studentId);
    Task<ICollection<Student>> ForSelectionListAsync();
    Task<List<Student>> ForInterviewsExportAsync(List<int> filterGrades, List<OfferingId> filterClasses, CancellationToken cancellationToken = default);

    void Insert(Student student);
}

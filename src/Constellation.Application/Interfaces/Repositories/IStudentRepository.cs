namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IStudentRepository
{
    Task<Student?> GetById(string studentId, CancellationToken cancellationToken = default);
    Task<Student?> GetWithSchoolById(string studentId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithSchool(CancellationToken cancellationToken = default);
    Task<List<Student>> GetListFromIds(List<string> studentIds, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForOfferingWithSchool(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithFamilyMemberships(CancellationToken cancellationToken = default);
    Task<bool> IsValidStudentId(string studentId, CancellationToken cancellationToken = default);
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

    Task<Student> GetForExistCheck(string id);
    Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate);
    Task<Student> ForEditAsync(string studentId);
    Task<Student> ForBulkUnenrolAsync(string studentId);
    Task<ICollection<Student>> ForSelectionListAsync();
    Task<List<Student>> ForInterviewsExportAsync(List<int> filterGrades, List<OfferingId> filterClasses, bool perFamily, bool resFamilyOnly, CancellationToken cancellationToken = default);
    Task<ICollection<Student>> WithoutAdobeConnectDetailsForUpdate();

    void Insert(Student student);
}

namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Application.DTOs;
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
    Task<Student?> GetById(string StudentId, CancellationToken cancellationToken = default);
    Task<Student?> GetWithSchoolById(string studentId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithSchool(CancellationToken cancellationToken = default);
    Task<List<Student>> GetListFromIds(List<string> studentIds, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForCourse(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentEnrolmentsForOfferingWithSchool(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsWithFamilyMemberships(CancellationToken cancellationToken = default);
    Task<bool> IsValidStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<List<Student>> GetFilteredStudents(List<OfferingId> OfferingIds, List<Grade> Grades, List<string> SchoolCodes, CancellationToken cancellationToken = default);
    Task<List<Student>> GetCurrentStudentsFromSchool(string SchoolCode, CancellationToken cancellationToken = default);
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

    Task <Student> GetForExistCheck(string id);
    Task<ICollection<Student>> AllActiveForFTECalculations();
    Task<ICollection<Student>> AllActiveForClassAuditAsync();
    Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate);
    Task<Student> ForEditAsync(string studentId);
    Task<Student> ForBulkUnenrolAsync(string studentId);
    Task<ICollection<Student>> ForSelectionListAsync();
    Task<Student> ForAttendanceQueryReport(string studentId);
    Task<List<Student>> ForInterviewsExportAsync(InterviewExportSelectionDto filter, CancellationToken cancellationToken = default);
    Task<bool> AnyWithId(string id);
    Task<ICollection<Student>> WithoutAdobeConnectDetailsForUpdate();
}
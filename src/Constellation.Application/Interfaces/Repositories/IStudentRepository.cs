using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        Task<Student?> GetById(string StudentId, CancellationToken cancellationToken = default);
        Task<Student?> GetWithSchoolById(string studentId, CancellationToken cancellationToken = default);
        Task<List<Student>> GetCurrentStudentsWithSchool(CancellationToken cancellationToken = default);
        Task<List<Student>> GetListFromIds(List<string> studentIds, CancellationToken cancellationToken = default);
        Task<List<Student>> GetCurrentEnrolmentsForOffering(int offeringId, CancellationToken cancellationToken = default);
        Task<List<Student>> GetCurrentEnrolmentsForOfferingWithSchool(int offeringId, CancellationToken cancellationToken = default);
        Task<List<Student>> GetCurrentStudentsWithFamilyMemberships(CancellationToken cancellationToken = default);
        Task<bool> IsValidStudentId(string studentId, CancellationToken cancellationToken = default);
        Task<List<Student>> GetFilteredStudents(List<int> OfferingIds, List<Grade> Grades, List<string> SchoolCodes, CancellationToken cancellationToken = default);
        Task<List<Student>> GetCurrentStudentsFromSchool(string SchoolCode, CancellationToken cancellationToken = default);

        Task <Student> GetForExistCheck(string id);
        Task<ICollection<Student>> AllWithAbsenceScanSettings();
        Task<ICollection<Student>> AllActiveAsync();
        Task<ICollection<Student>> AllEnrolledInCourse(int courseId);
        Task<ICollection<Student>> ForPTOFile(Expression<Func<Student, bool>> predicate);
        Task<ICollection<Student>> AllActiveForFTECalculations();
        Task<ICollection<Student>> AllActiveForClassAuditAsync();
        Task<Student> ForDetailDisplayAsync(string id);
        Task<ICollection<Student>> ForListAsync(Expression<Func<Student, bool>> predicate);
        Task<Student> ForEditAsync(string studentId);
        Task<Student> ForBulkUnenrolAsync(string studentId);
        Task<ICollection<Student>> ForSelectionListAsync();
        Task<Student> ForAttendanceQueryReport(string studentId);
        Task<List<Student>> ForInterviewsExportAsync(InterviewExportSelectionDto filter, CancellationToken cancellationToken = default);
        Task<bool> AnyWithId(string id);
        Task<Student> ForDeletion(string id);
        Task<ICollection<Student>> ForAttendanceReports();
        Task<ICollection<Student>> WithoutAdobeConnectDetailsForUpdate();
        Task<ICollection<Student>> ForAbsenceScan(Grade grade);
        Task<ICollection<Student>> ForTrackItSync();
    }
}
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ICourseOfferingRepository
    {
        Task<Faculty?> GetOfferingFaculty(int offeringId, CancellationToken cancellationToken = default);
        CourseOffering WithDetails(int id);
        CourseOffering WithFilter(Expression<Func<CourseOffering, bool>> predicate);
        Task<CourseOffering> GetForExistCheck(int id);
        ICollection<CourseOffering> All();
        ICollection<CourseOffering> AllWithFilter(Expression<Func<CourseOffering, bool>> predicate);
        ICollection<CourseOffering> AllCurrentOfferings();
        ICollection<CourseOffering> AllFutureOfferings();
        ICollection<CourseOffering> AllPastOfferings();
        ICollection<CourseOffering> AllFromFaculty(Faculty faculty);
        ICollection<CourseOffering> AllFromGrade(Grade grade);
        ICollection<CourseOffering> AllForStudent(string id);
        ICollection<CourseOffering> AllCurrentAndFutureForStudent(string id);
        ICollection<CourseOffering> AllCurrentForStudent(string id);
        ICollection<CourseOffering> AllForTeacher(string id);
        ICollection<Staff> AllClassTeachers(int id);
        Task<ICollection<CourseOffering>> ForSelectionAsync();
        Task<ICollection<CourseOffering>> FromGradeForBulkEnrolAsync(Grade grade);
        Task<CourseOffering> ForEnrolmentAsync(int id);
        Task<ICollection<CourseOffering>> AllForTeacherAsync(string id);
        Task<ICollection<CourseOffering>> ForListAsync(Expression<Func<CourseOffering, bool>> predicate);
        Task<CourseOffering> ForDetailDisplayAsync(int id);
        Task<CourseOffering> ForEditAsync(int id);
        Task<CourseOffering> ForSessionEditAsync(int id);
        Task<CourseOffering> ForRollCreationAsync(int id);
        Task<CourseOffering> ForCoverCreationAsync(int id);
        Task<ICollection<Staff>> AllTeachersForCoverCreationAsync(int id);
        Task<CourseOffering> ForSessionCreationAsync(int id);
        Task<bool> AnyWithId(int id);
        Task<ICollection<CourseOffering>> ForTeacherAndDates(string staffId, ICollection<int> dates);
        Task<CourseOffering> GetFromYearAndName(int year, string name);
    }
}
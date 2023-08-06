#nullable enable
namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface ICourseRepository
{
    Task<Course?> GetById(int courseId, CancellationToken cancellationToken = default);
    Task<List<Course>> GetAll(CancellationToken cancellationToken = default);
    Task<Course?> GetByLessonId(SciencePracLessonId lessonId, CancellationToken cancellationToken = default);

    Course WithDetails(int id);
    Course WithFilter(Expression<Func<Course, bool>> predicate);
    ICollection<Course> All();
    ICollection<Course> AllWithFilter(Expression<Func<Course, bool>> predicate);
    ICollection<Course> AllFromFaculty(Guid facultyId);
    ICollection<Course> AllFromGrade(Grade grade);
    ICollection<Course> AllWithActiveOfferings();
    ICollection<Course> AllWithoutActiveOfferings();
    //Task<IDictionary<int, string>> AllForLessonsPortal();
    Task<Course> WithOfferingsForLessonsPortal(int courseId);
    Task<ICollection<Course>> ForListAsync(Expression<Func<Course, bool>> predicate);
    Task<Course> ForDetailDisplayAsync(int id);
    Task<Course> ForEditAsync(int id);
    Task<ICollection<Course>> ForSelectionAsync();
    Task<bool> AnyWithId(int id);
}
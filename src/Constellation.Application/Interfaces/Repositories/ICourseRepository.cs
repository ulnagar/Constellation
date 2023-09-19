#nullable enable
namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICourseRepository
{
    Task<Course?> GetById(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Course>> GetAll(CancellationToken cancellationToken = default);
    Task<Course?> GetByLessonId(SciencePracLessonId lessonId, CancellationToken cancellationToken = default);
    Task<Course?> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Course>> GetByGrade(Grade grade, CancellationToken cancellationToken = default);
    Task<Course> ForEditAsync(CourseId id);
    Task<bool> AnyWithId(CourseId id);
    void Insert(Course course);
}
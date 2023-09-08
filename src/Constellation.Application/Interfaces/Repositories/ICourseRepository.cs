#nullable enable
namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICourseRepository
{
    Task<Course?> GetById(int courseId, CancellationToken cancellationToken = default);
    Task<List<Course>> GetAll(CancellationToken cancellationToken = default);
    Task<Course?> GetByLessonId(SciencePracLessonId lessonId, CancellationToken cancellationToken = default);
    Task<Course?> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<Course> ForEditAsync(int id);
    Task<bool> AnyWithId(int id);
}
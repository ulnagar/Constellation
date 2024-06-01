namespace Constellation.Core.Models.Enrolments.Repositories;

using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEnrolmentRepository
{
    Task<List<Enrolment>> GetCurrentByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<int> GetCurrentCountByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<List<Enrolment>> GetCurrentByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<int> GetCurrentCountByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<Enrolment>> GetCurrentByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    void Insert(Enrolment enrolment);
}
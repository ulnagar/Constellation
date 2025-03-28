namespace Constellation.Core.Models.OfferingEnrolments.Repositories;

using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IOfferingEnrolmentRepository
{
    Task<List<OfferingEnrolment>> GetCurrentByStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<int> GetCurrentCountByStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<OfferingEnrolment>> GetCurrentByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<int> GetCurrentCountByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<List<OfferingEnrolment>> GetCurrentByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    void Insert(OfferingEnrolment offeringEnrolment);
}
namespace Constellation.Core.Models.Faculties.Repositories;

using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IFacultyRepository
{
    Task<List<Faculty>> GetAll(CancellationToken cancellationToken = default);
    Task<Faculty> GetById(FacultyId facultyId, CancellationToken cancellationToken = default);
    Task<List<Faculty>> GetCurrentForStaffMember(string staffId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default);
    Task<Faculty> GetByCourseId(CourseId courseId, CancellationToken cancellationToken = default);
    Task<Faculty> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Faculty>> GetListFromIds(List<FacultyId> facultyIds, CancellationToken cancellationToken = default);
    void Insert(Faculty faculty);
}

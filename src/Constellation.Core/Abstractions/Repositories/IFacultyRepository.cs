namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IFacultyRepository
{
    Task<Faculty> GetById(Guid FacultyId, CancellationToken cancellationToken = default);
    Task<List<Faculty>> GetCurrentForStaffMember(string staffId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default);
    Task<Faculty> GetByOfferingId(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Faculty>> GetListFromIds(List<Guid> facultyIds, CancellationToken cancellationToken = default);
    void Insert(Faculty faculty);
}

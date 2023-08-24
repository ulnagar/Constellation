namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IFamilyRepository
{
    Task<bool> DoesFamilyWithEmailExist(EmailAddress email, CancellationToken cancellationToken = default);
    Task<List<Family>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<Family> GetFamilyBySentralId(string SentralId, CancellationToken cancellationToken = default);
    Task<Family> GetFamilyById(FamilyId Id, CancellationToken cancellationToken = default);
    Task<List<Family>> GetFamiliesByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<bool> DoesEmailBelongToParentOrFamily(string email, CancellationToken cancellationToken = default);
    Task<Dictionary<string, bool>> GetStudentIdsFromFamilyWithEmail(string email, CancellationToken cancellation = default);
    Task<int> CountOfParentsWithEmailAddress(string email, CancellationToken cancellationToken = default);
    void Insert(Family family);
}

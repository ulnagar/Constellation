namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Families;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IFamilyRepository
{
    Task<Family?> GetFamilyBySentralId(string SentralId, CancellationToken cancellationToken = default);
    Task<Family?> GetFamilyById(Guid Id, CancellationToken cancellationToken = default);
    Task<List<Family>> GetFamiliesByStudentId(string studentId, CancellationToken cancellationToken = default);
    Task<bool> DoesEmailBelongToParentOrFamily(string email, CancellationToken cancellationToken = default);
    Task<List<string>> GetStudentIdsFromFamilyWithEmail(string email, CancellationToken cancellation = default);
    void Insert(Family family);
}

namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Families;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStudentFamilyRepository
{
    Task<bool> DoesEmailBelongToParentOrFamily(string email, CancellationToken cancellationToken = default);
    Task<List<string>> GetStudentIdsFromFamilyWithEmail(string email, CancellationToken cancellation = default);
    void Insert(Family family);
}

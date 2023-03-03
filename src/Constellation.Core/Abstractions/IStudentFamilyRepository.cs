namespace Constellation.Core.Abstractions;

using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStudentFamilyRepository
{
    Task<List<StudentFamily>> GetFamilyWithEmail(string email, CancellationToken cancellationToken = default);
    Task<List<string>> GetStudentIdsFromFamilyWithEmail(string email, CancellationToken cancellation = default);
}

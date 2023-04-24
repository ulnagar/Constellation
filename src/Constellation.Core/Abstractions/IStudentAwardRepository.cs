namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Awards;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStudentAwardRepository
{
    Task<List<StudentAward>> GetByStudentId(string studentId, CancellationToken cancellationToken = default);

    void Insert(StudentAward studentAward);
}

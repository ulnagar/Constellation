namespace Constellation.Core.Abstractions.Repositories;

using Models.Awards;
using Models.Identifiers;
using Models.Students.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStudentAwardRepository
{
    Task<StudentAward> GetById(StudentAwardId awardId, CancellationToken cancellationToken = default);
    Task<List<StudentAward>> GetAll(CancellationToken cancellationToken = default);
    Task<List<StudentAward>> GetByStudentId(StudentId studentId, CancellationToken cancellationToken = default);
    Task<List<StudentAward>> GetFromYear(int Year, CancellationToken cancellationToken = default);
    Task<List<StudentAward>> GetFromRecentMonths(int Months, CancellationToken cancellationToken = default);
    Task<List<StudentAward>> GetToRecentCount(int Count, CancellationToken cancellationToken = default);

    void Insert(StudentAward studentAward);
}

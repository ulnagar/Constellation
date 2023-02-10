namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Covers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IClassCoverRepository
{
    Task<List<ClassCover>> GetAllCurrentAndUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForCurrentCalendarYear(CancellationToken cancellationToken = default);
    Task<ClassCover?> GetById(Guid CoverId, CancellationToken cancellationToken = default);
    Task<List<string>> GetCurrentCoveringTeachersForOffering(int offeringId, CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllWithCasualId(int casualId, CancellationToken cancellationToken = default);
    void Insert(ClassCover cover);
}

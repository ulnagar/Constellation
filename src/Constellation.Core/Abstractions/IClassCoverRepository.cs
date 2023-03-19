namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IClassCoverRepository
{
    Task<List<ClassCover>> GetAllCurrentAndUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForCurrentCalendarYear(CancellationToken cancellationToken = default);
    Task<ClassCover?> GetById(ClassCoverId CoverId, CancellationToken cancellationToken = default);
    Task<List<string>> GetCurrentCoveringTeachersForOffering(int offeringId, CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllWithCasualId(CasualId casualId, CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForDateAndOfferingId(DateOnly coverDate, int OfferingId, CancellationToken cancellationToken = default);
    void Insert(ClassCover cover);
}

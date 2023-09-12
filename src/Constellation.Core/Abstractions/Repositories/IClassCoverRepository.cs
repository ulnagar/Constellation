namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IClassCoverRepository
{
    Task<List<ClassCover>> GetAllCurrentAndUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForCurrentCalendarYear(CancellationToken cancellationToken = default);
    Task<ClassCover> GetById(ClassCoverId CoverId, CancellationToken cancellationToken = default);
    Task<List<string>> GetCurrentCoveringTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllWithCasualId(CasualId casualId, CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForDateAndOfferingId(DateOnly coverDate, OfferingId OfferingId, CancellationToken cancellationToken = default);
    void Insert(ClassCover cover);
}

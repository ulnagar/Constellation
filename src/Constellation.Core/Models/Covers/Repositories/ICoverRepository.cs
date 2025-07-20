namespace Constellation.Core.Models.Covers.Repositories;

using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Models.StaffMembers.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICoverRepository
{

    Task<List<Cover>> GetAllCurrentAndUpcoming(CancellationToken cancellationToken = default);
    Task<List<Cover>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<List<Cover>> GetAllUpcoming(CancellationToken cancellationToken = default);
    Task<List<Cover>> GetAllForCurrentCalendarYear(CancellationToken cancellationToken = default);
    Task<Cover> GetById(CoverId CoverId, CancellationToken cancellationToken = default);

    Task<List<string>> GetCurrentTeacherEmailsForAccessProvisioning(OfferingId offeringId, CancellationToken cancellationToken = default);
    
    Task<List<Cover>> GetAllWithCasualId(CasualId casualId, CancellationToken cancellationToken = default);
    Task<List<Cover>> GetAllForDateAndOfferingId(DateOnly coverDate, OfferingId OfferingId, CancellationToken cancellationToken = default);
    Task<List<Cover>> GetCurrentForStaff(StaffId staffId, CancellationToken cancellationToken = default);
    void Insert(Cover cover);
}

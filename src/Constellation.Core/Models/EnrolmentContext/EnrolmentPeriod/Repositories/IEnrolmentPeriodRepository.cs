namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;

using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using System.Collections.Generic;

public interface IEnrolmentPeriodRepository
{
    Task<List<EnrolmentPeriod>> GetAllEnrolmentPeriods(CancellationToken cancellationToken = default);
    Task<List<EnrolmentPeriod>> GetCurrentEnrolmentPeriods(CancellationToken cancellationToken = default);
    Task<EnrolmentPeriod?> GetEnrolmentPeriodById(EnrolmentPeriodId id, CancellationToken cancellationToken = default);

    void Insert(EnrolmentPeriod period);
}

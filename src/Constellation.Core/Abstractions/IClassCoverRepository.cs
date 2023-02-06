using Constellation.Core.Models.Covers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Core.Abstractions;
public interface IClassCoverRepository
{
    Task<List<ClassCover>> GetAllCurrentAndUpcoming(CancellationToken cancellationToken = default);
    Task<List<ClassCover>> GetAllForCurrentCalendarYear(CancellationToken cancellationToken = default);
    Task<ClassCover?> GetById(Guid CoverId, CancellationToken cancellationToken = default);

    void Insert(ClassCover cover);
}

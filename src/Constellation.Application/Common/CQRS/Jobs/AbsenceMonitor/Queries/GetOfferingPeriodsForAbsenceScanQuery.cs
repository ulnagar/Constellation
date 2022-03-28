using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries
{
    public class GetOfferingPeriodsForAbsenceScanQuery : IRequest<ICollection<TimetablePeriod>>
    {
        public int OfferingId { get; set; }
        public DateTime InstanceDate { get; set; }
        public int PeriodDay { get; set; }
    }

    public class GetOfferingPeriodsForAbsenceScanQueryHander : IRequestHandler<GetOfferingPeriodsForAbsenceScanQuery, ICollection<TimetablePeriod>>
    {
        private readonly IAppDbContext _context;

        public GetOfferingPeriodsForAbsenceScanQueryHander(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<TimetablePeriod>> Handle(GetOfferingPeriodsForAbsenceScanQuery request, CancellationToken cancellationToken)
        {
            var periods = await _context.Sessions
                .Where(session => session.OfferingId == request.OfferingId &&
                    session.DateCreated < request.InstanceDate &&
                    (!session.IsDeleted || session.DateDeleted.Value.Date > request.InstanceDate) &&
                    session.Period.Day == request.PeriodDay)
                .Select(session => session.Period)
                .ToListAsync(cancellationToken: cancellationToken);

            return periods;
        }
    }
}

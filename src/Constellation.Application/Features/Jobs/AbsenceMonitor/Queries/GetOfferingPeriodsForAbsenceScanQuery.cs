using Constellation.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetOfferingPeriodsForAbsenceScanQuery : IRequest<ICollection<TimetablePeriod>>
    {
        public int OfferingId { get; set; }
        public DateTime InstanceDate { get; set; }
        public int PeriodDay { get; set; }
    }
}

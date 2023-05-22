using Constellation.Core.Models.Absences;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetUnexplainedAbsencesForDigestQuery : IRequest<ICollection<Absence>>
    {
        public string StudentId { get; set; }
        public string Type { get; set; }
        public int AgeInWeeks { get; set; }
    }
}

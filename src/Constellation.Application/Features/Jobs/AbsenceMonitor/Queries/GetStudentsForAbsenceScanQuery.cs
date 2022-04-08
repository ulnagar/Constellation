using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Core.Enums;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentsForAbsenceScanQuery : IRequest<ICollection<StudentForAbsenceScan>>
    {
        public Grade Grade { get; set; }
    }
}

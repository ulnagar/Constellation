using Constellation.Core.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetRecentAbsencesForStudentQuery : IRequest<ICollection<Absence>>
    {
        public string StudentId { get; set; }
        public string AbsenceType { get; set; }
    }
}

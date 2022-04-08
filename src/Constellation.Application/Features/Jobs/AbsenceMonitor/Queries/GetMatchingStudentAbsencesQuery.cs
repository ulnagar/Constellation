using Constellation.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetMatchingStudentAbsencesQuery : IRequest<ICollection<Absence>>
    {
        public string StudentId { get; set; }
        public DateTime AbsenceDate { get; set; }
        public int AbsenceOffering { get; set; }
        public string AbsenceTimeframe { get; set; }
    }
}

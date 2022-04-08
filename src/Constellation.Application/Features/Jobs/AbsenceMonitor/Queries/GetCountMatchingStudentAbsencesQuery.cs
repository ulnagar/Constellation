using MediatR;
using System;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetCountMatchingStudentAbsencesQuery : IRequest<int>
    {
        public string StudentId { get; set; }
        public DateTime AbsenceDate { get; set; }
        public int AbsenceOffering { get; set; }
        public string AbsenceTimeframe { get; set; }
    }
}

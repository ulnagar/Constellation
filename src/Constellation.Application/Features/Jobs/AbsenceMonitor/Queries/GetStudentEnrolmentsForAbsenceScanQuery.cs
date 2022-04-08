using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentEnrolmentsForAbsenceScanQuery : IRequest<ICollection<StudentEnrolmentsForAbsenceScan>>
    {
        public string StudentId { get; set; }
        public DateTime InstanceDate { get; set; }
        public int PeriodDay { get; set; }
    }
}

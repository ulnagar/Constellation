using Constellation.Core.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Commands
{
    public class AddNewStudentAbsencesCommand : IRequest
    {
        public ICollection<Absence> Absences { get; set; }
    }
}

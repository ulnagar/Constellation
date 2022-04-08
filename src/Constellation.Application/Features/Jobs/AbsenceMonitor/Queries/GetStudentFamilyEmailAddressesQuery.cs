using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentFamilyEmailAddressesQuery : IRequest<ICollection<string>>
    {
        public string StudentId { get; set; }
    }
}

using MediatR;

namespace Constellation.Application.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentSentralIdQuery : IRequest<string?>
    {
        public string StudentId { get; set; }
    }
}

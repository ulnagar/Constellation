using Constellation.Application.Features.Portal.School.ScienceRolls.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.ScienceRolls.Commands
{
    public class SubmitScienceLessonRollCommandHandler : IRequestHandler<SubmitScienceLessonRollCommand>
    {
        public SubmitScienceLessonRollCommandHandler()
        {

        }

        public async Task<Unit> Handle(SubmitScienceLessonRollCommand request, CancellationToken cancellationToken)
        {
            return Unit.Value;
        }
    }
}

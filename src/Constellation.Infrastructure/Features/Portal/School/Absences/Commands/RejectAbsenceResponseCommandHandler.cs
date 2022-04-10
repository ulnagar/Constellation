using Constellation.Application.Features.Portal.School.Absences.Commands;
using Constellation.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Commands
{
    public class RejectAbsenceResponseCommandHandler : IRequestHandler<RejectAbsenceResponseCommand>
    {
        private readonly IAbsenceService _absenceService;

        public RejectAbsenceResponseCommandHandler(IAbsenceService absenceService)
        {
            _absenceService = absenceService;
        }

        public async Task<Unit> Handle(RejectAbsenceResponseCommand request, CancellationToken cancellationToken)
        {
            await _absenceService.RecordCoordinatorVerificationOfPartialExplanation(request.ResponseId, false, request.Comment, request.UserEmail);

            return Unit.Value;
        }
    }
}

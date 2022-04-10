using Constellation.Application.Features.Portal.School.Absences.Commands;
using Constellation.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Commands
{
    public class VerifyAbsenceResponseCommandHandler : IRequestHandler<VerifyAbsenceResponseCommand>
    {
        private readonly IAbsenceService _absenceService;

        public VerifyAbsenceResponseCommandHandler(IAbsenceService absenceService)
        {
            _absenceService = absenceService;
        }

        public async Task<Unit> Handle(VerifyAbsenceResponseCommand request, CancellationToken cancellationToken)
        {
            await _absenceService.RecordCoordinatorVerificationOfPartialExplanation(request.ResponseId, true, request.Comment, request.UserEmail);

            return Unit.Value;
        }
    }
}

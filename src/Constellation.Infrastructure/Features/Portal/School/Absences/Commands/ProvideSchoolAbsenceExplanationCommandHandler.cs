using Constellation.Application.Features.Portal.School.Absences.Commands;
using Constellation.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Commands
{
    public class ProvideSchoolAbsenceExplanationCommandHandler : IRequestHandler<ProvideSchoolAbsenceExplanationCommand>
    {
        private readonly IAbsenceService _absenceService;

        public ProvideSchoolAbsenceExplanationCommandHandler(IAbsenceService absenceService)
        {
            _absenceService = absenceService;
        }

        public async Task<Unit> Handle(ProvideSchoolAbsenceExplanationCommand request, CancellationToken cancellationToken)
        {
            await _absenceService.CreateSingleCoordinatorExplanation(request.AbsenceId, request.Comment, request.UserEmail);

            return Unit.Value;
        }
    }
}

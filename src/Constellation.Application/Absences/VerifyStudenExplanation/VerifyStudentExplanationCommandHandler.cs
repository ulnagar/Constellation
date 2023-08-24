namespace Constellation.Application.Absences.VerifyStudenExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class VerifyStudentExplanationCommandHandler
    : ICommandHandler<VerifyStudentExplanationCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public VerifyStudentExplanationCommandHandler(
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<VerifyStudentExplanationCommand>();
    }
    public async Task<Result> Handle(VerifyStudentExplanationCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Response response = absence.Responses.FirstOrDefault(response => response.Id == request.ResponseId);

        if (response is null)
        {
            _logger.Warning("Could not find response with Id {response_id}", request.ResponseId);

            return Result.Failure(DomainErrors.Absences.Response.NotFound(request.ResponseId));
        }

        response.VerifyResponse(request.UserEmail, request.Comment);

        absence.ResponseConfirmed(response.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

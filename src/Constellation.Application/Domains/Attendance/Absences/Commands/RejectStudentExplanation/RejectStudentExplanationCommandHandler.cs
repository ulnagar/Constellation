namespace Constellation.Application.Domains.Attendance.Absences.Commands.RejectStudentExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Core.Models.Absences.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RejectStudentExplanationCommandHandler
    : ICommandHandler<RejectStudentExplanationCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RejectStudentExplanationCommandHandler(
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RejectStudentExplanationCommand>();
    }

    public async Task<Result> Handle(RejectStudentExplanationCommand request, CancellationToken cancellationToken)
    {
        Absence? absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure(AbsenceErrors.NotFound(request.AbsenceId));
        }

        Response? response = absence.Responses.FirstOrDefault(response => response.Id == request.ResponseId);

        if (response is null)
        {
            _logger.Warning("Could not find response with Id {response_id}", request.ResponseId);

            return Result.Failure(AbsenceResponseErrors.NotFound(request.ResponseId));
        }

        response.RejectResponse(request.UserEmail, request.Comment);

        absence.ResponseConfirmed(response.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

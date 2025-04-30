namespace Constellation.Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAbsenceResponseFromStudentCommandHandler
    : ICommandHandler<CreateAbsenceResponseFromStudentCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAbsenceResponseFromStudentCommandHandler(
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateAbsenceResponseFromStudentCommand>();
    }

    public async Task<Result> Handle(CreateAbsenceResponseFromStudentCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence in database when looking with Id {id}", request.AbsenceId);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Result result = absence.AddResponse(
            ResponseType.Student,
            absence.StudentId.ToString(),
            request.Explanation);

        if (result.IsFailure)
        {
            _logger.Warning("Failed to add student response to absence");

            return result;
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

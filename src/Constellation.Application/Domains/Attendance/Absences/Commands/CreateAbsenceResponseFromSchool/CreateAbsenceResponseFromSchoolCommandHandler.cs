namespace Constellation.Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Core.Models.Absences.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAbsenceResponseFromSchoolCommandHandler
    : ICommandHandler<CreateAbsenceResponseFromSchoolCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAbsenceResponseFromSchoolCommandHandler(
        IAbsenceRepository absenceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateAbsenceResponseFromSchoolCommand>();
    }

    public async Task<Result> Handle(CreateAbsenceResponseFromSchoolCommand request, CancellationToken cancellationToken)
    {
        Absence? absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id} when trying to save explanation {@request}", request.AbsenceId, request);

            return Result.Failure(AbsenceErrors.NotFound(request.AbsenceId));
        }

        absence.AddResponse(
            ResponseType.Coordinator,
            request.UserEmail,
            request.Comment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

namespace Constellation.Application.Absences.CreateAbsenceResponseFromSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAbsenceResponseFromSchoolCommandHandler
    : ICommandHandler<CreateAbsenceResponseFromSchoolCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAbsenceResponseFromSchoolCommandHandler(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateAbsenceResponseFromSchoolCommand>();
    }
    public async Task<Result> Handle(CreateAbsenceResponseFromSchoolCommand request, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id} when trying to save explanation {@request}", request.AbsenceId, request);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        absence.AddResponse(
            ResponseType.Coordinator,
            request.UserEmail,
            request.Comment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

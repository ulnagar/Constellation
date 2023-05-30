namespace Constellation.Application.Absences.RejectStudentExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Org.BouncyCastle.Crypto;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RejectStudentExplanationCommandHandler
    : ICommandHandler<RejectStudentExplanationCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RejectStudentExplanationCommandHandler(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RejectStudentExplanationCommand>();
    }

    public async Task<Result> Handle(RejectStudentExplanationCommand request, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not find absence with Id {id}", request.AbsenceId);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        var response = await _responseRepository.GetById(request.ResponseId, cancellationToken);

        if (response is null)
        {
            _logger.Warning("Could not find response with Id {response_id}", request.ResponseId);

            return Result.Failure(DomainErrors.Absences.Response.NotFound(request.ResponseId));
        }

        response.RejectResponse(request.UserEmail, request.Comment);

        absence.ResponseConfirmed(response.Id);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

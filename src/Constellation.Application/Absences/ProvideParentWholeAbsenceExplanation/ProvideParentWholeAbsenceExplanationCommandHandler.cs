﻿namespace Constellation.Application.Absences.ProvideParentWholeAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProvideParentWholeAbsenceExplanationCommandHandler : ICommandHandler<ProvideParentWholeAbsenceExplanationCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger _logger;

    public ProvideParentWholeAbsenceExplanationCommandHandler(
        ILogger logger,
        IFamilyRepository familyRepository,
        IUnitOfWork unitOfWork,
        IAbsenceRepository absenceRepository)
    {
        _logger = logger.ForContext<ProvideParentWholeAbsenceExplanationCommand>();
        _familyRepository = familyRepository;
        _unitOfWork = unitOfWork;
        _absenceRepository = absenceRepository;
    }

    public async Task<Result> Handle(ProvideParentWholeAbsenceExplanationCommand request, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(request.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger
                .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Absences.Absence.NotFound(request.AbsenceId), true)
                .Information("Could not find absence with Id {id} when processing request {@request}", request.AbsenceId, request);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceId));
        }

        Dictionary<string, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        if (!studentIds.ContainsKey(absence.StudentId))
        {
            _logger
                .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Information("Parent {parent} does not have permission to view details of student {student} when processing request {@request}", request.ParentEmail, absence.StudentId, request);

            return Result.Failure(DomainErrors.Permissions.Unauthorised);
        }

        absence.AddResponse(
            ResponseType.Parent,
            request.ParentEmail,
            request.Comment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

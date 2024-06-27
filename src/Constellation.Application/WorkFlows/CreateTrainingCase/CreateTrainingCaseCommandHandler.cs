﻿namespace Constellation.Application.WorkFlows.CreateTrainingCase;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.WorkFlow;
using Core.Errors;
using Core.Models;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Models.WorkFlow.Repositories;
using Core.Models.WorkFlow.Services;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingCaseCommandHandler
: ICommandHandler<CreateTrainingCaseCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ICaseRepository _caseRepository;
    private readonly ICaseService _caseService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTrainingCaseCommandHandler(
        IStaffRepository staffRepository,
        ITrainingModuleRepository moduleRepository,
        ICaseRepository caseRepository,
        ICaseService caseService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _moduleRepository = moduleRepository;
        _caseRepository = caseRepository;
        _caseService = caseService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateTrainingCaseCommand>();
    }

    public async Task<Result> Handle(CreateTrainingCaseCommand request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(CreateTrainingCaseCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        TrainingModule module = await _moduleRepository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(CreateTrainingCaseCommand), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.ModuleId), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        TrainingCompletion? completion = request.CompletionId != null
            ? module.Completions.Where(completion => completion.StaffId == staffMember.StaffId).MaxBy(completion => completion.CompletedDate)
            : null;

        Result<Case> caseResult = await _caseService.CreateTrainingCase(
            staffMember.StaffId,
            module.Id,
            completion?.Id,
            cancellationToken);

        if (caseResult.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTrainingCaseCommand), request, true)
                .ForContext(nameof(Error), caseResult.Error, true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(caseResult.Error);
        }

        _caseRepository.Insert(caseResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

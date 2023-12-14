﻿namespace Constellation.Application.Training.Modules.UpdateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingModuleCommandHandler
    : ICommandHandler<UpdateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        if (module is null)
            return Result.Failure(TrainingErrors.Module.NotFound(request.Id));

        module.Update(
            request.Name,
            request.Expiry,
            request.Url);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

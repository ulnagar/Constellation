﻿namespace Constellation.Application.Domains.Training.Commands.MarkTrainingCompletionRecordDeleted;

using Abstractions.Messaging;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MarkTrainingCompletionRecordDeletedCommandHandler
    : ICommandHandler<MarkTrainingCompletionRecordDeletedCommand>
{
    private readonly ITrainingModuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkTrainingCompletionRecordDeletedCommandHandler(
        ITrainingModuleRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<MarkTrainingCompletionRecordDeletedCommand>();
    }

    public async Task<Result> Handle(MarkTrainingCompletionRecordDeletedCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _repository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);

            return Result.Failure(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger.Warning("Could not find Training Completion with Id {id}", request.CompletionId);

            return Result.Failure(TrainingCompletionErrors.NotFound(request.CompletionId));
        }

        record.Delete();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

namespace Constellation.Application.MandatoryTraining.MarkTrainingCompletionRecordDeleted;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Core.Models.MandatoryTraining.Errors;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MarkTrainingCompletionRecordDeletedCommandHandler
    : ICommandHandler<MarkTrainingCompletionRecordDeletedCommand>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public MarkTrainingCompletionRecordDeletedCommandHandler(
        ITrainingModuleRepository moduleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<MarkTrainingCompletionRecordDeletedCommand>();
    }

    public async Task<Result> Handle(MarkTrainingCompletionRecordDeletedCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _moduleRepository.GetById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);

            return Result.Failure(TrainingErrors.Module.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            _logger.Warning("Could not find Training Completion with Id {id}", request.CompletionId);

            return Result.Failure(TrainingErrors.Completion.NotFound(request.CompletionId));
        }

        record.Delete();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

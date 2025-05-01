namespace Constellation.Application.Domains.Training.Commands.UpdateTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingModuleCommandHandler
    : ICommandHandler<UpdateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateTrainingModuleCommand>();
    }

    public async Task<Result> Handle(UpdateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingModuleCommand), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.Id), true)
                .Warning("Failed to update Training Module");

            return Result.Failure(TrainingModuleErrors.NotFound(request.Id));
        }

        module.Update(
            request.Name,
            request.Expiry,
            request.Url);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

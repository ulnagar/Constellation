namespace Constellation.Application.Training.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateTrainingModuleCommandHandler
    : ICommandHandler<ReinstateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ReinstateTrainingModuleCommand>();
    }

    public async Task<Result> Handle(ReinstateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(ReinstateTrainingModuleCommand), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.Id), true)
                .Warning("Failed to reinstate Training Module");

            return Result.Failure(TrainingModuleErrors.NotFound(request.Id));
        }

        module.Reinstate();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

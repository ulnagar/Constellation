namespace Constellation.Application.Training.Modules.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateTrainingModuleCommandHandler
    : ICommandHandler<ReinstateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReinstateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingRepository = trainingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReinstateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        if (module is null)
            return Result.Failure(TrainingErrors.Module.NotFound(request.Id));

        module.Reinstate();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

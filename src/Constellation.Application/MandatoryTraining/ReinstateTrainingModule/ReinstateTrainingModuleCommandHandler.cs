namespace Constellation.Application.MandatoryTraining.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateTrainingModuleCommandHandler 
    : ICommandHandler<ReinstateTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReinstateTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReinstateTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);

        if (module is null)
            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.Id));

        module.Reinstate();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

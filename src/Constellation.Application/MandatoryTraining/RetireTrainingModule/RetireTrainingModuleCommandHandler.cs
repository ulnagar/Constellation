namespace Constellation.Application.MandatoryTraining.RetireTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RetireTrainingModuleCommandHandler 
    : ICommandHandler<RetireTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RetireTrainingModuleCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RetireTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);

        if (module is null)
            return Result.Failure(DomainErrors.MandatoryTraining.Module.NotFound(request.Id));

        module.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

namespace Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesModuleAllowNotRequiredResponseQueryHandler 
    : IQueryHandler<DoesModuleAllowNotRequiredResponseQuery, bool>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;

    public DoesModuleAllowNotRequiredResponseQueryHandler(
        ITrainingModuleRepository trainingModuleRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
    }

    public async Task<Result<bool>> Handle(DoesModuleAllowNotRequiredResponseQuery request, CancellationToken cancellationToken)
    {
        var module = await _trainingModuleRepository.GetById(request.ModuleId);
        
        if (module is null)
        {
            return Result.Failure<bool>(DomainErrors.MandatoryTraining.Module.NotFound(request.ModuleId));
        }

        return module.CanMarkNotRequired;
    }
}

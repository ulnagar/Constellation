namespace Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.MandatoryTraining.Errors;
using Constellation.Core.Shared;
using Core.Models.MandatoryTraining;
using System.Linq;
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
        TrainingModule module = await _trainingModuleRepository.GetById(request.ModuleId, cancellationToken);
        
        if (module is null)
        {
            return Result.Failure<bool>(TrainingErrors.Module.NotFound(request.ModuleId));
        }

        bool isRequired = module
            .Roles
            .Any(role => role.Role.Members.Any(member => member.StaffId == request.StaffId));

        return !isRequired;
    }
}

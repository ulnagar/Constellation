namespace Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models.MandatoryTraining;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingModuleEditContextQueryHandler 
    : IQueryHandler<GetTrainingModuleEditContextQuery, ModuleEditContextDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;

    public GetTrainingModuleEditContextQueryHandler(
        ITrainingModuleRepository trainingModuleRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
    }

    public async Task<Result<ModuleEditContextDto>> Handle(GetTrainingModuleEditContextQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);

        return new ModuleEditContextDto
        {
            Id = module.Id,
            Name = module.Name,
            Url = module.Url,
            Expiry = module.Expiry
        };
    }
}

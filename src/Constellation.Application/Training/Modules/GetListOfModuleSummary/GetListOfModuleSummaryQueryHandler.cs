namespace Constellation.Application.Training.Modules.GetListOfModuleSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Shared;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfModuleSummaryQueryHandler
    : IQueryHandler<GetListOfModuleSummaryQuery, List<ModuleSummaryDto>>
{
    private readonly ITrainingModuleRepository _trainingRepository;

    public GetListOfModuleSummaryQueryHandler(
        ITrainingModuleRepository trainingRepository)
    {
        _trainingRepository = trainingRepository;
    }

    public async Task<Result<List<ModuleSummaryDto>>> Handle(GetListOfModuleSummaryQuery request, CancellationToken cancellationToken)
    {
        List<ModuleSummaryDto> data = new();

        List<TrainingModule> modules = await _trainingRepository.GetAllModules(cancellationToken);

        if (modules is null)
        {
            return data;
        }

        foreach (TrainingModule module in modules)
        {
            if (module.IsDeleted) continue;

            ModuleSummaryDto entry = new ModuleSummaryDto
            {
                Id = module.Id,
                Name = module.Name,
                Expiry = module.Expiry.GetDisplayName(),
                Url = module.Url,
                IsActive = !module.IsDeleted
            };

            data.Add(entry);
        }

        return data;
    }
}

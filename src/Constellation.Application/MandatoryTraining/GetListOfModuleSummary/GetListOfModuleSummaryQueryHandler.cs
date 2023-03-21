namespace Constellation.Application.MandatoryTraining.GetListOfModuleSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfModuleSummaryQueryHandler 
    : IQueryHandler<GetListOfModuleSummaryQuery, List<ModuleSummaryDto>>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;

    public GetListOfModuleSummaryQueryHandler(
        ITrainingModuleRepository trainingModuleRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
    }

    public async Task<Result<List<ModuleSummaryDto>>> Handle(GetListOfModuleSummaryQuery request, CancellationToken cancellationToken)
    {
        List<ModuleSummaryDto> data = new();

        var modules = await _trainingModuleRepository.GetCurrentModules(cancellationToken);

        if (modules is null)
        {
            return data;
        }

        foreach (var module in modules)
        {
            var entry = new ModuleSummaryDto
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

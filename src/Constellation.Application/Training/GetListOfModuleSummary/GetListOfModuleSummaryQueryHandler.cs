﻿namespace Constellation.Application.Training.GetListOfModuleSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Shared;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Models;
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
            return data;

        foreach (TrainingModule module in modules)
        {
            ModuleSummaryDto entry = new(
                module.Id,
                module.Name,
                !module.IsDeleted,
                module.Expiry.GetDisplayName(),
                module.Url);

            data.Add(entry);
        }

        return data;
    }
}

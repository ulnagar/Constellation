namespace Constellation.Application.Features.Common.Queries;

using Core.Models.Training;
using Core.Models.Training.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetTrainingModulesAsDictionaryQuery : IRequest<Dictionary<Guid, string>>
{
}

public class GetTrainingModulesAsDictionaryQueryHandler : IRequestHandler<GetTrainingModulesAsDictionaryQuery, Dictionary<Guid, string>>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;


    public GetTrainingModulesAsDictionaryQueryHandler(
        ITrainingModuleRepository trainingModuleRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
    }

	public async Task<Dictionary<Guid, string>> Handle(GetTrainingModulesAsDictionaryQuery request, CancellationToken cancellationToken)
	{
		List<TrainingModule> modules = await _trainingModuleRepository
            .GetAllModules(cancellationToken);

		return modules
            .Where(module => !module.IsDeleted)
			.OrderBy(module => module.Name)
			.ToDictionary(module => module.Id.Value, module => module.Name);
	}
}

namespace Constellation.Application.Training.GetTrainingModuleEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Shared;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingModuleEditContextQueryHandler
    : IQueryHandler<GetTrainingModuleEditContextQuery, ModuleEditContextDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly ILogger _logger;

    public GetTrainingModuleEditContextQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        ILogger logger)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _logger = logger.ForContext<GetTrainingModuleEditContextQuery>();
    }

    public async Task<Result<ModuleEditContextDto>> Handle(GetTrainingModuleEditContextQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _trainingModuleRepository.GetModuleById(request.Id, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(GetTrainingModuleEditContextQuery), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve edit context for Training Module");

            return Result.Failure<ModuleEditContextDto>(TrainingModuleErrors.NotFound(request.Id));
        }

        return new ModuleEditContextDto
        {
            Id = module.Id,
            Name = module.Name,
            Url = module.Url,
            Expiry = module.Expiry
        };
    }
}

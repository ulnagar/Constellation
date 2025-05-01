namespace Constellation.Application.Domains.Training.Queries.DoesModuleAllowNotRequiredResponse;

using Abstractions.Messaging;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesModuleAllowNotRequiredResponseQueryHandler
    : IQueryHandler<DoesModuleAllowNotRequiredResponseQuery, bool>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ILogger _logger;

    public DoesModuleAllowNotRequiredResponseQueryHandler(
        ITrainingModuleRepository moduleRepository,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _logger = logger.ForContext<DoesModuleAllowNotRequiredResponseQuery>();
    }

    public async Task<Result<bool>> Handle(DoesModuleAllowNotRequiredResponseQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _moduleRepository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(DoesModuleAllowNotRequiredResponseQuery), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.ModuleId), true)
                .Warning("Failed to check Module for Assignee");

            return Result.Failure<bool>(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        return module.Assignees.Any(assignee => assignee.StaffId == request.StaffId);
    }
}

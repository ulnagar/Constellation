namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordEditContextQueryHandler 
    : IQueryHandler<GetCompletionRecordEditContextQuery, CompletionRecordEditContextDto>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly ILogger _logger;

    public GetCompletionRecordEditContextQueryHandler(
        ITrainingModuleRepository moduleRepository,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _logger = logger;
    }

    public async Task<Result<CompletionRecordEditContextDto>> Handle(GetCompletionRecordEditContextQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _moduleRepository.GetById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);
            return Result.Failure<CompletionRecordEditContextDto>(DomainErrors.MandatoryTraining.Module.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        CompletionRecordEditContextDto entity = new()
        {
            Id = record.Id,
            TrainingModuleId = record.TrainingModuleId,
            StaffId = record.StaffId,
            CompletedDate = record.CompletedDate.HasValue ? record.CompletedDate.Value : DateTime.Today,
            NotRequired = record.NotRequired
        };

        return entity;
    }
}

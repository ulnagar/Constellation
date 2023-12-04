namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordEditContextQueryHandler 
    : IQueryHandler<GetCompletionRecordEditContextQuery, CompletionRecordEditContextDto>
{
    private readonly ITrainingModuleRepository _repository;
    private readonly ILogger _logger;

    public GetCompletionRecordEditContextQueryHandler(
        ITrainingModuleRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<CompletionRecordEditContextDto>> Handle(GetCompletionRecordEditContextQuery request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _repository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger.Warning("Could not find Training Module with Id {id}", request.ModuleId);
            return Result.Failure<CompletionRecordEditContextDto>(TrainingErrors.Module.NotFound(request.ModuleId));
        }

        TrainingCompletion record = module.Completions.FirstOrDefault(record => record.Id == request.CompletionId);

        if (record is null)
        {
            return Result.Failure<CompletionRecordEditContextDto>(TrainingErrors.Completion.NotFound(request.CompletionId));
        }

        CompletionRecordEditContextDto entity = new()
        {
            Id = record.Id,
            TrainingModuleId = record.TrainingModuleId,
            StaffId = record.StaffId,
            CompletedDate = record.CompletedDate.ToDateTime(TimeOnly.MinValue)
        };

        return entity;
    }
}

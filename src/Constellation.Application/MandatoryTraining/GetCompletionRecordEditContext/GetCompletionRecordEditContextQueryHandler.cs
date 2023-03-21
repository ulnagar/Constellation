namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCompletionRecordEditContextQueryHandler 
    : IQueryHandler<GetCompletionRecordEditContextQuery, CompletionRecordEditContextDto>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;

    public GetCompletionRecordEditContextQueryHandler(
        ITrainingCompletionRepository trainingCompletionRepository)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
    }

    public async Task<Result<CompletionRecordEditContextDto>> Handle(GetCompletionRecordEditContextQuery request, CancellationToken cancellationToken)
    {
        var record = await _trainingCompletionRepository.GetById(request.Id, cancellationToken);

        var entity = new CompletionRecordEditContextDto
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

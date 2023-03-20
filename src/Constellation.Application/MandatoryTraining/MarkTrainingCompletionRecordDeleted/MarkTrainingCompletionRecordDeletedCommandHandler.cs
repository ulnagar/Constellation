namespace Constellation.Application.MandatoryTraining.MarkTrainingCompletionRecordDeleted;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MarkTrainingCompletionRecordDeletedCommandHandler
    : ICommandHandler<MarkTrainingCompletionRecordDeletedCommand>
{
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkTrainingCompletionRecordDeletedCommandHandler(
        ITrainingCompletionRepository trainingCompletionRepository,
        IUnitOfWork unitOfWork)
    {
        _trainingCompletionRepository = trainingCompletionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkTrainingCompletionRecordDeletedCommand request, CancellationToken cancellationToken)
    {
        var record = await _trainingCompletionRepository.GetById(request.RecordId, cancellationToken);

        if (record is null)
            return Result.Failure(DomainErrors.MandatoryTraining.Completion.NotFound(request.RecordId));

        record.Delete();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

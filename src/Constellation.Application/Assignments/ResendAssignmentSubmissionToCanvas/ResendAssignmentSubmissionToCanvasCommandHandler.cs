namespace Constellation.Application.Assignments.ResendAssignmentSubmissionToCanvas;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ResendAssignmentSubmissionToCanvasCommandHandler
    : ICommandHandler<ResendAssignmentSubmissionToCanvasCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResendAssignmentSubmissionToCanvasCommandHandler(
        IAssignmentRepository assignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResendAssignmentSubmissionToCanvasCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure(DomainErrors.Assignments.Assignment.NotFound(request.AssignmentId));

        var result = assignment.ReuploadSubmissionToCanvas(request.SubmissionId);

        if (result.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}

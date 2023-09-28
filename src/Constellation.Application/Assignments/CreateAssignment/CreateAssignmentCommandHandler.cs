namespace Constellation.Application.Assignments.CreateAssignment;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

public class CreateAssignmentCommandHandler
 : ICommandHandler<CreateAssignmentCommand>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssignmentCommandHandler(
        IAssignmentRepository assignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        CanvasAssignment record = CanvasAssignment.Create(
            request.CourseId,
            request.Name,
            request.CanvasAssignmentId,
            request.DueDate,
            request.LockDate,
            request.UnlockDate,
            request.DelayForwarding,
            request.ForwardDate,
            request.AllowedAttempts);

        _assignmentRepository.Insert(record);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

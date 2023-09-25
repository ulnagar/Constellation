namespace Constellation.Infrastructure.Features.Subjects.Assignments.Commands;

using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Features.Subject.Assignments.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assignments;

public class CreateCanvasAssignmentCommandHandler : IRequestHandler<CreateCanvasAssignmentCommand, ValidateableResponse>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCanvasAssignmentCommandHandler(
        IAssignmentRepository assignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidateableResponse> Handle(CreateCanvasAssignmentCommand request, CancellationToken cancellationToken)
    {
        var record = CanvasAssignment.Create(
            request.CourseId,
            request.Name,
            request.CanvasId,
            request.DueDate,
            request.LockDate,
            request.UnlockDate,
            request.AllowedAttempts);

        _assignmentRepository.Insert(record);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return new ValidateableResponse();
    }
}

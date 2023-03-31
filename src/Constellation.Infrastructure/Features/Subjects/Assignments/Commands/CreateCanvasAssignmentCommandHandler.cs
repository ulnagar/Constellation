using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Features.Subject.Assignments.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Identifiers;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Commands;

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
            new AssignmentId(),
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

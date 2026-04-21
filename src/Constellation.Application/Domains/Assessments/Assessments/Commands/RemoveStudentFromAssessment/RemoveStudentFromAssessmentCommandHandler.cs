namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveStudentFromAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveStudentFromAssessmentCommandHandler
: ICommandHandler<RemoveStudentFromAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStudentFromAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveStudentFromAssessmentCommand>();
    }

    public async Task<Result> Handle(RemoveStudentFromAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(RemoveStudentFromAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to remove student from Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        assessment.RemoveStudent(request.StudentId);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

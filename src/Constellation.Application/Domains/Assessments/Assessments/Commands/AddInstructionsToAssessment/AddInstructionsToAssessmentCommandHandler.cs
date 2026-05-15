namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddInstructionsToAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class AddInstructionsToAssessmentCommandHandler
    : ICommandHandler<AddInstructionsToAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddInstructionsToAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddInstructionsToAssessmentCommand>();
    }

    public async Task<Result> Handle(AddInstructionsToAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AddInstructionsToAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to add Instructions to Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentInstruction instructions = new(
            assessment.Id,
            request.Category,
            request.Instructions);

        assessment.AddInstructions(instructions);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
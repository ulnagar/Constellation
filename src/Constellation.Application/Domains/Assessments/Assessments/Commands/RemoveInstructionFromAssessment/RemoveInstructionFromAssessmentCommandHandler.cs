namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveInstructionFromAssessment;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveInstructionFromAssessmentCommandHandler
: ICommandHandler<RemoveInstructionFromAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveInstructionFromAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveInstructionFromAssessmentCommand>();
    }

    public async Task<Result> Handle(RemoveInstructionFromAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(RemoveInstructionFromAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to remove Instructions from Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentInstruction? instruction = assessment.Instructions.FirstOrDefault(entry => entry.Id == request.InstructionId);

        if (instruction is null)
            return Result.Success();

        assessment.RemoveInstructions(instruction);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

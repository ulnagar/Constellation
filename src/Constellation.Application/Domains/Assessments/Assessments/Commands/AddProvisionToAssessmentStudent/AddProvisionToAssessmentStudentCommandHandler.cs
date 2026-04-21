namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddProvisionToAssessmentStudent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Serilog;

internal sealed class AddProvisionToAssessmentStudentCommandHandler
: ICommandHandler<AddProvisionToAssessmentStudentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddProvisionToAssessmentStudentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddProvisionToAssessmentStudentCommand>();
    }

    public async Task<Result> Handle(AddProvisionToAssessmentStudentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AddProvisionToAssessmentStudentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to add provision to student from Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<Provision> provisions = await _assessmentRepository.GetProvisionsFromList(request.ProvisionIds, cancellationToken);

        foreach (Provision provision in provisions)
            assessment.AddStudentProvision(request.StudentId, provision);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

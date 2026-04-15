namespace Constellation.Application.Domains.Assessments.Provisions.Commands.RemoveStudentProvision;

using Abstractions.Messaging;
using Constellation.Application.Domains.Assessments.Provisions.Models;
using Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisionById;
using Constellation.Core.Models.Assessments.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveStudentProvisionCommandHandler
: ICommandHandler<RemoveStudentProvisionCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStudentProvisionCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveStudentProvisionCommand>();
    }

    public async Task<Result> Handle(RemoveStudentProvisionCommand request, CancellationToken cancellationToken)
    {
        StudentProvision? provision = await _assessmentRepository.GetStudentProvisionById(request.Id, cancellationToken);

        if (provision is null)
        {
            _logger
                .ForContext(nameof(GetStudentProvisionByIdQuery), request, true)
                .ForContext(nameof(Error), StudentProvisionErrors.NotFound(request.Id), true)
                .Warning("Failed to remove Student Provision");

            return Result.Failure(StudentProvisionErrors.NotFound(request.Id));
        }

        provision.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

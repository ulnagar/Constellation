namespace Constellation.Application.Domains.Assessments.Provisions.Commands.UpdateAssessmentProvision;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpdateAssessmentProvisionCommandHandler
: ICommandHandler<UpdateAssessmentProvisionCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAssessmentProvisionCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAssessmentProvisionCommand request, CancellationToken cancellationToken)
    {
        Provision? provision = await _assessmentRepository.GetProvisionById(request.Id, cancellationToken);

        if (provision is null)
        {
            _logger
                .ForContext(nameof(UpdateAssessmentProvisionCommand), request, true)
                .ForContext(nameof(Error), ProvisionErrors.NotFound(request.Id), true)
                .Warning("Failed to update Assessment Provision");

            return Result.Failure(ProvisionErrors.NotFound(request.Id));
        }

        provision.Update(request.Code, request.Description);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

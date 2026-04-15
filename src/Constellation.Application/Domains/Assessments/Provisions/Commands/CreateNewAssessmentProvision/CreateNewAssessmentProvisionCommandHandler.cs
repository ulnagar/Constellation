namespace Constellation.Application.Domains.Assessments.Provisions.Commands.CreateNewAssessmentProvision;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class CreateNewAssessmentProvisionCommandHandler
: ICommandHandler<CreateNewAssessmentProvisionCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNewAssessmentProvisionCommandHandler(
        IAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateNewAssessmentProvisionCommand request, CancellationToken cancellationToken)
    {
        bool existing = await _assessmentRepository.DoesProvisionCodeExist(request.Code, cancellationToken);

        if (existing)
        {
            _logger
                .ForContext(nameof(CreateNewAssessmentProvisionCommand), request, true)
                .ForContext(nameof(Error), ProvisionCodeErrors.AlreadyExists, true)
                .Warning("Failed to create Assessment Provision");

            return Result.Failure(ProvisionCodeErrors.AlreadyExists);
        }

        Provision provision = new(request.Code, request.Description);

        _assessmentRepository.Insert(provision);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

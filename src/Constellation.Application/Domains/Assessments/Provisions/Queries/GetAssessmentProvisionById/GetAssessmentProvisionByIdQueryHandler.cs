namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisionById;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetAssessmentProvisionByIdQueryHandler
: IQueryHandler<GetAssessmentProvisionByIdQuery, AssessmentProvisionResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentProvisionByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<AssessmentProvisionResponse>> Handle(GetAssessmentProvisionByIdQuery request, CancellationToken cancellationToken)
    {
        Provision? provision = await _assessmentRepository.GetProvisionById(request.Id, cancellationToken);

        if (provision is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentProvisionByIdQuery), request, true)
                .ForContext(nameof(Error), ProvisionErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Assessment Provision");
            
            return Result.Failure<AssessmentProvisionResponse>(ProvisionErrors.NotFound(request.Id));
        }

        return new AssessmentProvisionResponse(provision.Id, provision.Code, provision.Description);
    }
}

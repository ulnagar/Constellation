namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisions;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetAssessmentProvisionsQueryHandler
: IQueryHandler<GetAssessmentProvisionsQuery, List<AssessmentProvisionResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentProvisionsQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<AssessmentProvisionResponse>>> Handle(GetAssessmentProvisionsQuery request, CancellationToken cancellationToken)
    {
        List<AssessmentProvisionResponse> response = [];

        List<Provision> provisions = await _assessmentRepository.GetProvisions(cancellationToken);

        foreach (var provision in provisions)
        {
            response.Add(new(provision.Id, provision.Code, provision.Description));
        }

        return response;
    }
}

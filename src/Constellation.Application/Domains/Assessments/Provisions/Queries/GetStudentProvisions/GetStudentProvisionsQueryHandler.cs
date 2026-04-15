namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisions;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetStudentProvisionsQueryHandler
: IQueryHandler<GetStudentProvisionsQuery, List<StudentProvisionResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetStudentProvisionsQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentProvisionResponse>>> Handle(GetStudentProvisionsQuery request, CancellationToken cancellationToken)
    {
        List<StudentProvisionResponse> response = [];

        List<StudentProvision> studentProvisions = await _assessmentRepository.GetStudentProvisions(cancellationToken);

        foreach (StudentProvision provision in studentProvisions)
        {
            response.Add(new(
                provision.Id,
                provision.ProvisionCode,
                provision.ProvisionDescription,
                provision.Student,
                provision.Year,
                provision.IsDeleted));
        }

        return response;
    }
}

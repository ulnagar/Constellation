namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetCurrentYearStudentProvisions;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentYearStudentProvisionsQueryHandler
: IQueryHandler<GetCurrentYearStudentProvisionsQuery, List<StudentProvisionResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetCurrentYearStudentProvisionsQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentProvisionResponse>>> Handle(GetCurrentYearStudentProvisionsQuery request, CancellationToken cancellationToken)
    {
        List<StudentProvisionResponse> response = [];

        List<StudentProvision> studentProvisions = await _assessmentRepository.GetStudentProvisionsFromCurrentYear(cancellationToken);

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

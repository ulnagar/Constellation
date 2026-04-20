namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Models;
using System.Collections.Generic;

internal sealed class GetCurrentAssessmentsQueryHandler
: IQueryHandler<GetCurrentAssessmentsQuery, List<AssessmentResponse>>
{
    private readonly IAssessmentRepository _assessmentRepository;

    public GetCurrentAssessmentsQueryHandler(
        IAssessmentRepository assessmentRepository)
    {
        _assessmentRepository = assessmentRepository;
    }

    public async Task<Result<List<AssessmentResponse>>> Handle(GetCurrentAssessmentsQuery request, CancellationToken cancellationToken)
    {
        List<AssessmentResponse> response = [];

        List<Assessment> assessments = await _assessmentRepository.GetCurrentAssessments(cancellationToken);

        foreach (Assessment assessment in assessments)
        {
            response.Add(new(
                assessment.Id,
                assessment.Name,
                assessment.CourseId,
                assessment.Course,
                assessment.Grade,
                assessment.DueDate,
                assessment.AvailableFrom,
                assessment.AvailableTo));
        }

        return response;
    }
}

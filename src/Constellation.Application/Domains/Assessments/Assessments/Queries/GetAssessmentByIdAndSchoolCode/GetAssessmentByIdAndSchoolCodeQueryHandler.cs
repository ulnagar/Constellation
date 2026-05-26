namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentByIdAndSchoolCode;

using Abstractions.Messaging;
using Constellation.Core.Models.Assessments;
using Constellation.Core.Models.Assessments.Repositories;
using Core.Models.Assessments.Errors;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetAssessmentByIdAndSchoolCodeQueryHandler
: IQueryHandler<GetAssessmentByIdAndSchoolCodeQuery, AssessmentDetailsResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentByIdAndSchoolCodeQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger
            .ForContext<GetAssessmentByIdAndSchoolCodeQuery>();
    }

    public async Task<Result<AssessmentDetailsResponse>> Handle(GetAssessmentByIdAndSchoolCodeQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentByIdAndSchoolCodeQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to retrieve Assessment");

            return Result.Failure<AssessmentDetailsResponse>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<AssessmentDetailsResponse.Student> students = [];

        foreach (AssessmentStudent student in assessment.Students.Where(student => student.SchoolCode == request.SchoolCode))
        {
            students.Add(new(
                student.StudentId,
                student.Student,
                student.StudentGrade,
                student.SchoolCode,
                student.SchoolName,
                student.Provisions.Select(entry => $"{entry.Code}: {entry.Description}").ToList(),
                student.IsDeleted));
        }

        return new AssessmentDetailsResponse(
            assessment.Id,
            assessment.Name,
            assessment.CourseId,
            assessment.Course,
            assessment.Grade,
            assessment.DueDate,
            assessment.AvailableFrom,
            assessment.AvailableTo,
            null,
            students,
            [],
            [],
            []);
    }
}

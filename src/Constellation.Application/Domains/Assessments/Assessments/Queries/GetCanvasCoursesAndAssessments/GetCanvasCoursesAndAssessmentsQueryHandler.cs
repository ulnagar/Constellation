namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Shared;
using DTOs;
using Interfaces.Gateways;
using LinkedSystems.Canvas.Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCanvasCoursesAndAssessmentsQueryHandler
: IQueryHandler<GetCanvasCoursesAndAssessmentsQuery, List<CanvasCourseWithAssessmentResponse>>
{
    private readonly ICanvasGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCanvasCoursesAndAssessmentsQueryHandler(
        ICanvasGateway gateway,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _gateway = gateway;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetCanvasCoursesAndAssessmentsQuery>();
    }

    public async Task<Result<List<CanvasCourseWithAssessmentResponse>>> Handle(GetCanvasCoursesAndAssessmentsQuery request, CancellationToken cancellationToken)
    {
        List<CanvasCourseWithAssessmentResponse> response = [];

        List<CourseListEntry> courses = await _gateway.GetAllCourses(_dateTime.CurrentYearAsString, cancellationToken);

        if (courses.Count == 0)
        {
            return response;
        }

        foreach (CourseListEntry course in courses)
        {
            List<CanvasCourseWithAssessmentResponse.Assessment> assessments = [];

            List<CanvasAssignmentDto> assignments = await _gateway.GetAllCourseAssignments(course.CourseCode, cancellationToken);

            foreach (CanvasAssignmentDto assignment in assignments)
            {
                if (assignment.LockDate.HasValue && assignment.LockDate.Value < DateTimeOffset.Now)
                    continue;

                assessments.Add(new(
                    assignment.CanvasId,
                    assignment.Name,
                    assignment.DueDate,
                    assignment.UnlockDate,
                    assignment.LockDate,
                    assignment.AllowedAttempts));
            }

            response.Add(new(
                course.CourseCode,
                course.Name,
                assessments));
        }

        return response;
    }
}

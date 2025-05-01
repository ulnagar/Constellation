namespace Constellation.Application.Domains.Assignments.Queries.GetCurrentAssignmentsListing;

using Abstractions.Messaging;
using Core.Extensions;
using Core.Models.Assignments;
using Core.Models.Assignments.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentAssignmentsListingQueryHandler
    : IQueryHandler<GetCurrentAssignmentsListingQuery, List<CurrentAssignmentSummaryResponse>>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;

    public GetCurrentAssignmentsListingQueryHandler(
        IAssignmentRepository assignmentRepository,
        ICourseRepository courseRepository)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<CurrentAssignmentSummaryResponse>>> Handle(GetCurrentAssignmentsListingQuery request, CancellationToken cancellationToken)
    {
        List<CurrentAssignmentSummaryResponse> response = new();

        List<CanvasAssignment> assignments = await _assignmentRepository.GetAllCurrentAndFuture(cancellationToken);

        if (assignments.Count == 0)
            return response;

        foreach (CanvasAssignment assignment in assignments)
        {
            Course course = await _courseRepository.GetById(assignment.CourseId, cancellationToken);

            if (course is null)
                continue;

            string courseName = $"Y{course.Grade.AsNumber()} {course.Name}";

            CurrentAssignmentSummaryResponse entry = new CurrentAssignmentSummaryResponse(
                assignment.Id,
                courseName,
                assignment.Name,
                DateOnly.FromDateTime(assignment.DueDate));

            response.Add(entry);
        }

        return response;
    }
}

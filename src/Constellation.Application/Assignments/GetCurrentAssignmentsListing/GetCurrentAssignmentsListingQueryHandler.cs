namespace Constellation.Application.Assignments.GetCurrentAssignmentsListing;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Shared;
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

        var assignments = await _assignmentRepository.GetAllCurrent(cancellationToken);

        if (assignments is null)
            return response;

        foreach (var assignment in assignments)
        {
            var course = await _courseRepository.GetById(assignment.CourseId, cancellationToken);

            if (course is null)
                continue;

            var courseName = $"Y{course.Grade.AsNumber()} {course.Name}";

            var entry = new CurrentAssignmentSummaryResponse(
                assignment.Id,
                courseName,
                assignment.Name,
                DateOnly.FromDateTime(assignment.DueDate));

            response.Add(entry);
        }

        return response;
    }
}

namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssignmentsByCourseQueryHandler
    : IQueryHandler<GetAssignmentsByCourseQuery, List<CourseAssignmentResponse>>
{
    private readonly IAssignmentRepository _assignmentRepository;

    public GetAssignmentsByCourseQueryHandler(
        IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<Result<List<CourseAssignmentResponse>>> Handle(GetAssignmentsByCourseQuery request, CancellationToken cancellationToken)
    {
        List<CourseAssignmentResponse> result = new();

        var assignments = await _assignmentRepository.GetByCourseId(request.CourseId, cancellationToken);

        if (assignments is null)
            return Result.Failure<List<CourseAssignmentResponse>>(DomainErrors.Assignments.Assignment.NotFoundByCourse(request.CourseId));

        // Only process assignments that the student hasn't already submitted the maximum number of attempts
        var validAssignments = assignments
            .Where(entry =>
                entry.AllowedAttempts <= 0 ||
                entry.Submissions.Count(submission => submission.StudentId == request.StudentId) < entry.AllowedAttempts)
            .ToList();

        foreach (var assignment in assignments)
        {
            var entry = new CourseAssignmentResponse(
                assignment.Id,
                assignment.Name,
                DateOnly.FromDateTime(assignment.DueDate));

            result.Add(entry);
        }

        return result;
    }
}

namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentsByCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Shared;
using Core.Models.Assessments.Archive;
using Core.Models.Assessments.Archive.Errors;
using Core.Models.Assessments.Archive.Repositories;
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

        List<CanvasAssignment> assignments = await _assignmentRepository.GetByCourseId(request.CourseId, cancellationToken);

        if (assignments.Count == 0)
            return Result.Failure<List<CourseAssignmentResponse>>(AssignmentErrors.NotFoundByCourse(request.CourseId));

        // Only process assignments that the student hasn't already submitted the maximum number of attempts
        List<CanvasAssignment> validAssignments = assignments
            .Where(entry =>
                (entry.AllowedAttempts <= 0 ||
                entry.Submissions.Count(submission => submission.StudentId == request.StudentId) < entry.AllowedAttempts) &&
                ((entry.LockDate.HasValue && entry.LockDate.Value >= DateTime.Now) || 
                (!entry.LockDate.HasValue && entry.DueDate >= DateTime.Now)))
            .ToList();

        foreach (CanvasAssignment assignment in validAssignments)
        {
            CourseAssignmentResponse entry = new(
                assignment.Id,
                assignment.Name,
                $"{assignment.Name} (Due: {assignment.DueDate.ToShortDateString()})",
                assignment.DueDate);

            result.Add(entry);
        }

        return result;
    }
}

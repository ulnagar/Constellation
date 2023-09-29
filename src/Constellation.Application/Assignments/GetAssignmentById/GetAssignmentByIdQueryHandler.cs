namespace Constellation.Application.Assignments.GetAssignmentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssignmentByIdQueryHandler
    : IQueryHandler<GetAssignmentByIdQuery, AssignmentResponse>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public GetAssignmentByIdQueryHandler(
        IAssignmentRepository assignmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _assignmentRepository = assignmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<AssignmentResponse>> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure<AssignmentResponse>(DomainErrors.Assignments.Assignment.NotFound(request.AssignmentId));

        List<AssignmentResponse.Submission> submissions = new();

        foreach (var submission in assignment.Submissions)
        {
            var student = await _studentRepository.GetById(submission.StudentId, cancellationToken);

            if (student is null)
                continue;

            var record = new AssignmentResponse.Submission(
                submission.Id,
                student.DisplayName,
                DateOnly.FromDateTime(submission.SubmittedOn),
                submission.Attempt);

            submissions.Add(record);
        }

        var course = await _courseRepository.GetById(assignment.CourseId, cancellationToken);

        if (course is null)
            return Result.Failure<AssignmentResponse>(CourseErrors.NotFound(assignment.CourseId));

        var courseName = $"Y{course.Grade.AsNumber()} {course.Name}";

        var entry = new AssignmentResponse(
            assignment.Id,
            course.Id,
            courseName,
            assignment.Name,
            DateOnly.FromDateTime(assignment.DueDate),
            (assignment.UnlockDate.HasValue ? DateOnly.FromDateTime(assignment.UnlockDate.Value) : null),
            (assignment.LockDate.HasValue ? DateOnly.FromDateTime(assignment.LockDate.Value) : null),
            assignment.AllowedAttempts,
            submissions);

        return entry;
    }
}

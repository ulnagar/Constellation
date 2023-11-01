namespace Constellation.Application.Assignments.GetAssignmentById;

using Abstractions.Messaging;
using Extensions;
using Interfaces.Repositories;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Core.Extensions;
using Core.Shared;
using Core.Models;
using Core.Models.Assignments;
using Core.Models.Assignments.Errors;
using Core.Models.Subjects;
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
        CanvasAssignment assignment = await _assignmentRepository.GetById(request.AssignmentId, cancellationToken);

        if (assignment is null)
            return Result.Failure<AssignmentResponse>(AssignmentErrors.NotFound(request.AssignmentId));

        List<AssignmentResponse.Submission> submissions = new();

        foreach (CanvasAssignmentSubmission submission in assignment.Submissions)
        {
            Student student = await _studentRepository.GetById(submission.StudentId, cancellationToken);

            if (student is null)
                continue;

            AssignmentResponse.Submission record = new(
                submission.Id,
                student.DisplayName,
                DateOnly.FromDateTime(submission.SubmittedOn),
                submission.Attempt);

            submissions.Add(record);
        }

        Course course = await _courseRepository.GetById(assignment.CourseId, cancellationToken);

        if (course is null)
            return Result.Failure<AssignmentResponse>(CourseErrors.NotFound(assignment.CourseId));

        string courseName = $"Y{course.Grade.AsNumber()} {course.Name}";

        AssignmentResponse entry = new(
            assignment.Id,
            course.Id,
            courseName,
            assignment.Name,
            DateOnly.FromDateTime(assignment.DueDate),
            (assignment.UnlockDate.HasValue ? DateOnly.FromDateTime(assignment.UnlockDate.Value) : null),
            (assignment.LockDate.HasValue ? DateOnly.FromDateTime(assignment.LockDate.Value) : null),
            assignment.DelayForwarding,
            assignment.ForwardingDate,
            assignment.AllowedAttempts,
            submissions);

        return entry;
    }
}

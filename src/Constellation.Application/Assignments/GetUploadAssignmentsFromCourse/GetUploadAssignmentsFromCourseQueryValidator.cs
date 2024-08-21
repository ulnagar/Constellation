namespace Constellation.Application.Assignments.GetUploadAssignmentsFromCourse;

using FluentValidation;

internal sealed class GetUploadAssignmentsFromCourseQueryValidator : AbstractValidator<GetUploadAssignmentsFromCourseQuery>
{
    public GetUploadAssignmentsFromCourseQueryValidator()
    {
        RuleFor(request => request.CourseId).NotEmpty();
    }
}
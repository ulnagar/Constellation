namespace Constellation.Application.Assignments.GetAssignmentsFromCourse;

using FluentValidation;

internal sealed class GetAssignmentsFromCourseQueryValidator : AbstractValidator<GetAssignmentsFromCourseQuery>
{
    public GetAssignmentsFromCourseQueryValidator()
    {
        RuleFor(request => request.CourseId).NotEmpty();
    }
}
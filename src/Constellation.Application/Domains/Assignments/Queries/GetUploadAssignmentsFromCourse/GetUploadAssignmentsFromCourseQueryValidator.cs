namespace Constellation.Application.Domains.Assignments.Queries.GetUploadAssignmentsFromCourse;

using FluentValidation;

internal sealed class GetUploadAssignmentsFromCourseQueryValidator : AbstractValidator<GetUploadAssignmentsFromCourseQuery>
{
    public GetUploadAssignmentsFromCourseQueryValidator()
    {
        RuleFor(request => request.CourseId).NotEmpty();
    }
}
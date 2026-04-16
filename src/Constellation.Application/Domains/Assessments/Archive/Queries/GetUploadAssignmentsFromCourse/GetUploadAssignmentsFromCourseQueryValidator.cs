namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetUploadAssignmentsFromCourse;

using FluentValidation;

internal sealed class GetUploadAssignmentsFromCourseQueryValidator : AbstractValidator<GetUploadAssignmentsFromCourseQuery>
{
    public GetUploadAssignmentsFromCourseQueryValidator()
    {
        RuleFor(request => request.CourseId).NotEmpty();
    }
}
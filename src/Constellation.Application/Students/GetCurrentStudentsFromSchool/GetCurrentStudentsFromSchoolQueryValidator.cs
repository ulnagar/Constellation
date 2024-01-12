namespace Constellation.Application.Students.GetCurrentStudentsFromSchool;

using FluentValidation;

internal sealed class GetCurrentStudentsFromSchoolQueryValidator 
    : AbstractValidator<GetCurrentStudentsFromSchoolQuery>
{
    public GetCurrentStudentsFromSchoolQueryValidator()
    {
        RuleFor(query => query.SchoolCode).NotEmpty();
    }
}
namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;

using FluentValidation;

internal sealed class GetCurrentStudentsFromSchoolQueryValidator 
    : AbstractValidator<GetCurrentStudentsFromSchoolQuery>
{
    public GetCurrentStudentsFromSchoolQueryValidator()
    {
        RuleFor(query => query.SchoolCode).NotEmpty();
    }
}
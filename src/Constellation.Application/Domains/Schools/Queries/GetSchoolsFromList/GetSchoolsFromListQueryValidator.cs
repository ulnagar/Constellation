namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsFromList;

using FluentValidation;

internal sealed class GetSchoolsFromListQueryValidator : AbstractValidator<GetSchoolsFromListQuery>
{
    public GetSchoolsFromListQueryValidator()
    {
        RuleFor(command => command.SchoolCodes).NotEmpty();
    }
}
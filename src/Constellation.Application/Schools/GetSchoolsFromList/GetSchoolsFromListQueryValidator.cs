namespace Constellation.Application.Schools.GetSchoolsFromList;

using FluentValidation;

internal sealed class GetSchoolsFromListQueryValidator : AbstractValidator<GetSchoolsFromListQuery>
{
    public GetSchoolsFromListQueryValidator()
    {
        RuleFor(command => command.SchoolCodes).NotEmpty();
    }
}
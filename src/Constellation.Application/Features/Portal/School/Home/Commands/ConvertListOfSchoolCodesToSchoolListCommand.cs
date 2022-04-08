using Constellation.Application.DTOs;
using FluentValidation;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Home.Commands
{
    public class ConvertListOfSchoolCodesToSchoolListCommand : IRequest<ICollection<SchoolDto>>
    {
        public ICollection<string> SchoolCodes { get; set; }
    }

    public class ConvertListOfSchoolCodesToSchoolListCommandValidator : AbstractValidator<ConvertListOfSchoolCodesToSchoolListCommand>
    {
        public ConvertListOfSchoolCodesToSchoolListCommandValidator()
        {
            RuleFor(command => command.SchoolCodes).NotEmpty();
        }
    }
}

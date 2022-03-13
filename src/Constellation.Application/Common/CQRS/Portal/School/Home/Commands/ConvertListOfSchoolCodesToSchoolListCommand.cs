using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Home.Commands
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

    public class ConvertListOfSchoolCodesToSchoolListCommandHandler : IRequestHandler<ConvertListOfSchoolCodesToSchoolListCommand, ICollection<SchoolDto>>
    {
        private readonly IAppDbContext _context;

        public ConvertListOfSchoolCodesToSchoolListCommandHandler(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<ICollection<SchoolDto>> Handle(ConvertListOfSchoolCodesToSchoolListCommand request, CancellationToken cancellationToken)
        {
            var returnData = new List<SchoolDto>();

            foreach (var schoolCode in request.SchoolCodes)
            {
                var school = await _context.Schools.FirstOrDefaultAsync(school => school.Code == schoolCode, cancellationToken);
                if (school != null)
                {
                    returnData.Add(new SchoolDto { Code = schoolCode, Name = school.Name });
                }
            }

            return returnData;
        }
    }
}

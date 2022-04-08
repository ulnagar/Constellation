using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Home.Queries
{
    public class GetLinkedSchoolCodesForUserQueryHandler : IRequestHandler<GetLinkedSchoolCodesForUserQuery, ICollection<string>>
    {
        private readonly IAppDbContext _context;

        public GetLinkedSchoolCodesForUserQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<string>> Handle(GetLinkedSchoolCodesForUserQuery request, CancellationToken cancellationToken)
        {
            return await _context.SchoolContacts
                .Where(contact => contact.EmailAddress == request.UserEmail)
                .SelectMany(contact => contact.Assignments.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.SchoolCode))
                .ToListAsync(cancellationToken);
        }
    }
}

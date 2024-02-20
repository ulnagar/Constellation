using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.SchoolContacts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Auth.Queries
{
    public class GetSchoolContactByEmailAddressQueryHandler : IRequestHandler<GetSchoolContactByEmailAddressQuery, SchoolContact>
    {
        private readonly IAppDbContext _context;

        public GetSchoolContactByEmailAddressQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<SchoolContact> Handle(GetSchoolContactByEmailAddressQuery request, CancellationToken cancellationToken)
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .SingleOrDefaultAsync(contact => contact.EmailAddress == request.EmailAddress, cancellationToken);
        }
    }
}

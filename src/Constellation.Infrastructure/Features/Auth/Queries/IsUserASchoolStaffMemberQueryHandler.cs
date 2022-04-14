using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Auth.Queries
{
    public class IsUserASchoolStaffMemberQueryHandler : IRequestHandler<IsUserASchoolStaffMemberQuery, bool>
    {
        private readonly IAppDbContext _context;

        public IsUserASchoolStaffMemberQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(IsUserASchoolStaffMemberQuery request, CancellationToken cancellationToken)
        {
            return await _context.Staff
                .AnyAsync(staff => request.EmailAddress.Contains(staff.PortalUsername) && !staff.IsDeleted);
        }
    }
}

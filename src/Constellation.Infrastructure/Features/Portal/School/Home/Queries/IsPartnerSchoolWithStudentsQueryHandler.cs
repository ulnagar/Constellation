using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Home.Queries
{
    public class IsPartnerSchoolWithStudentsQueryHandler : IRequestHandler<IsPartnerSchoolWithStudentsQuery, bool>
    {
        private readonly IAppDbContext _context;

        public IsPartnerSchoolWithStudentsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(IsPartnerSchoolWithStudentsQuery request, CancellationToken cancellationToken)
        {
            var school = await _context.Schools
                .SingleOrDefaultAsync(school => school.Code == request.SchoolCode && school.Students.Any(student => !student.IsDeleted), cancellationToken);

            return school != null;
        }
    }
}

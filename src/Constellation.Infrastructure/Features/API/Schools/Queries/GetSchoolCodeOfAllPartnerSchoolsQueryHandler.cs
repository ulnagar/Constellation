using Constellation.Application.Features.API.Schools.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.API.Schools.Queries
{
    public class GetSchoolCodeOfAllPartnerSchoolsQueryHandler : IRequestHandler<GetSchoolCodeOfAllPartnerSchoolsQuery, ICollection<string>>
    {
        private readonly IAppDbContext _context;

        public GetSchoolCodeOfAllPartnerSchoolsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<string>> Handle(GetSchoolCodeOfAllPartnerSchoolsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Schools
                .Where(school => school.Students.Any(student => !student.IsDeleted) || school.Staff.Any(staff => !staff.IsDeleted))
                .Select(school => school.Code)
                .ToListAsync();
        }
    }
}

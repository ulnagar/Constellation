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
    public class GetAllPartnerSchoolCodesQueryHandler : IRequestHandler<GetAllPartnerSchoolCodesQuery, ICollection<string>>
    {
        private readonly IAppDbContext _context;

        public GetAllPartnerSchoolCodesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<string>> Handle(GetAllPartnerSchoolCodesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Schools
                .Where(school => school.Students.Any(student => !student.IsDeleted))
                .Select(school => school.Code)
                .ToListAsync(cancellationToken);
        }
    }
}

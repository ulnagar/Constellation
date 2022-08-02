using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Partners.Schools.Models;
using Constellation.Application.Features.Partners.Schools.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Partners.Schools.Queries
{
    public class GetSchoolsForListQueryHandler : IRequestHandler<GetSchoolsForListQuery, List<SchoolForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetSchoolsForListQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SchoolForList>> Handle(GetSchoolsForListQuery request, CancellationToken cancellationToken)
        {
            return await _context.Schools
                .Where(school => 
                    (request.WithStudents.HasValue && request.WithStudents.Value == false && !school.Students.Any(student => !student.IsDeleted)) ||
                    (request.WithStudents.HasValue && request.WithStudents.Value == true && school.Students.Any(student => !student.IsDeleted)) ||
                    (request.WithStaff.HasValue && request.WithStaff.Value == false && !school.Staff.Any(staff => !staff.IsDeleted)) ||
                    (request.WithStaff.HasValue && request.WithStaff.Value == true && school.Staff.Any(staff => !staff.IsDeleted))
                )
                .ProjectTo<SchoolForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

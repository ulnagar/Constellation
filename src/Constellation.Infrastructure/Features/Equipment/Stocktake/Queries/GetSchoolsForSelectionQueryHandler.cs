using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Equipment.Stocktake.Models;
using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Equipment.Stocktake.Queries
{
    public class GetSchoolsForSelectionQueryHandler : IRequestHandler<GetSchoolsForSelectionQuery, ICollection<PartnerSchoolForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetSchoolsForSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<PartnerSchoolForDropdownSelection>> Handle(GetSchoolsForSelectionQuery request, CancellationToken cancellationToken)
        {
            return await _context.Schools
                .Where(school => school.Staff.Any(staff => !staff.IsDeleted) || school.Students.Any(student => !student.IsDeleted))
                .ProjectTo<PartnerSchoolForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

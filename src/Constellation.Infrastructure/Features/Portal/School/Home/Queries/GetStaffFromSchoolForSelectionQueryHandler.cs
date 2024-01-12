using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.StaffMembers.GetStaffFromSchool;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Home.Queries
{
    public class GetStaffFromSchoolForSelectionQueryHandler : IRequestHandler<GetStaffFromSchoolQuery, ICollection<StaffFromSchoolForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStaffFromSchoolForSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StaffFromSchoolForDropdownSelection>> Handle(GetStaffFromSchoolQuery request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staff
                .Where(staff => staff.SchoolCode == request.SchoolCode && !staff.IsDeleted)
                .ProjectTo<StaffFromSchoolForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return staff;
        }
    }
}

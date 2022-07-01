using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Features.Portal.School.Home.Models;
using Constellation.Application.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Home.Queries
{
    public class GetStaffForSelectionQueryHandler : IRequestHandler<GetStaffForSelectionQuery, ICollection<StaffFromSchoolForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStaffForSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StaffFromSchoolForDropdownSelection>> Handle(GetStaffForSelectionQuery request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staff
                .Where(staff => !staff.IsDeleted)
                .ProjectTo<StaffFromSchoolForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return staff;
        }
    }
}

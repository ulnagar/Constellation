using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Home.Models;
using Constellation.Application.Features.Portal.School.Home.Queries;
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
    public class GetStudentsFromSchoolForSelectionQueryHandler : IRequestHandler<GetStudentsFromSchoolForSelectionQuery, ICollection<StudentFromSchoolForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsFromSchoolForSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentFromSchoolForDropdownSelection>> Handle(GetStudentsFromSchoolForSelectionQuery request, CancellationToken cancellationToken)
        {
            var students = await _context.Students
                .Where(student => student.SchoolCode == request.SchoolCode && !student.IsDeleted)
                .ProjectTo<StudentFromSchoolForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return students;
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Courses.Models;
using Constellation.Application.Features.Subject.Courses.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Courses.Queries
{
    public class GetCourseForDropdownSelectionQueryHandler : IRequestHandler<GetCoursesForDropdownSelectionQuery, ICollection<CourseForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetCourseForDropdownSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ICollection<CourseForDropdownSelection>> Handle(GetCoursesForDropdownSelectionQuery request, CancellationToken cancellationToken)
        {
            return await _context.Courses
                .ProjectTo<CourseForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

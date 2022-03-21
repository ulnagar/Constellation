using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Subject.Courses.Queries
{
    public class GetCoursesForDropdownSelectionQuery : IRequest<ICollection<CourseForDropdownSelection>>
    {
    }

    public class CourseForDropdownSelection : IMapFrom<Course>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Faculty { get; set; }

        public string DisplayName => $"{Grade} {Name}";
    }

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

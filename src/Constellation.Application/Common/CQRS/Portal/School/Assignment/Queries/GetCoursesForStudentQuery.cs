using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Queries
{
    public class GetCoursesForStudentQuery : IRequest<ICollection<StudentCoursesForDropdownSelection>>
    {
        public string StudentId { get; set; }
    }

    public class StudentCoursesForDropdownSelection : IMapFrom<Course>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string DisplayName => $"{Grade} {Name}";
    }

    public class GetCoursesForStudentQueryHandler : IRequestHandler<GetCoursesForStudentQuery, ICollection<StudentCoursesForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetCoursesForStudentQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ICollection<StudentCoursesForDropdownSelection>> Handle(GetCoursesForStudentQuery request, CancellationToken cancellationToken)
        {
            var courses = await _context.Enrolments
                .Where(enrolment => 
                    enrolment.StudentId == request.StudentId &&
                    !enrolment.IsDeleted &&
                    enrolment.Offering.EndDate >= DateTime.Now)
                .ProjectTo<StudentCoursesForDropdownSelection>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return courses;
        }
    }
}

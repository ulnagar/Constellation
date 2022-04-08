using Constellation.Application.DTOs;
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
    public class GetStudentsFromSchoolQueryHandler : IRequestHandler<GetStudentsFromSchoolQuery, ICollection<StudentDto>>
    {
        private readonly IAppDbContext _context;

        public GetStudentsFromSchoolQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<StudentDto>> Handle(GetStudentsFromSchoolQuery request, CancellationToken cancellationToken)
        {
            var students = await _context.Students.Where(student => student.SchoolCode == request.SchoolCode && !student.IsDeleted).ToListAsync();
            return students.Select(StudentDto.ConvertFromStudent).ToList();
        }
    }
}

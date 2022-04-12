using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Reports.Models;
using Constellation.Application.Features.Portal.School.Reports.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Reports.Queries
{
    public class GetStudentReportListForSchoolQueryHandler : IRequestHandler<GetStudentReportListForSchoolQuery, ICollection<StudentReportForDownload>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentReportListForSchoolQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentReportForDownload>> Handle(GetStudentReportListForSchoolQuery request, CancellationToken cancellationToken)
        {
            return await _context.StudentReports
                .Where(report => !report.Student.IsDeleted && report.Student.SchoolCode == request.SchoolCode)
                .ProjectTo<StudentReportForDownload>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

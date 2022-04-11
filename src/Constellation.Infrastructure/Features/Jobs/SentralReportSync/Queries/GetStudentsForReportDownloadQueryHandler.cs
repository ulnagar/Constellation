using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Jobs.SentralReportSync.Models;
using Constellation.Application.Features.Jobs.SentralReportSync.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.SentralReportSync
{
    public class GetStudentsForReportDownloadQueryHandler : IRequestHandler<GetStudentsForReportDownloadQuery, ICollection<StudentForReportDownload>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsForReportDownloadQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentForReportDownload>> Handle(GetStudentsForReportDownloadQuery request, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Where(student => !student.IsDeleted)
                .ProjectTo<StudentForReportDownload>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentsForAbsenceScanQueryHandler : IRequestHandler<GetStudentsForAbsenceScanQuery, ICollection<StudentForAbsenceScan>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsForAbsenceScanQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentForAbsenceScan>> Handle(GetStudentsForAbsenceScanQuery request, CancellationToken cancellationToken)
        {
            var students = await _context.Students
                .Where(student => student.IncludeInAbsenceNotifications && !student.IsDeleted && student.CurrentGrade == request.Grade)
                .ProjectTo<StudentForAbsenceScan>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);

            return students;
        }
    }
}

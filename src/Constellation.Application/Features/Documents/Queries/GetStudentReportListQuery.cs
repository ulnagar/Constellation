namespace Constellation.Application.Features.Documents.Queries;

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

public class GetStudentReportListQuery : IRequest<ICollection<StudentReportForList>>
{
    public string StudentId { get; set; }
}

public class StudentReportForList : IMapFrom<StudentReport>
{
    public string PublishId { get; set; }
    public Guid Id { get; set; }
    public string Year { get; set; }
    public string ReportingPeriod { get; set; }
}

public class GetStudentReportListQueryHandler : IRequestHandler<GetStudentReportListQuery, ICollection<StudentReportForList>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetStudentReportListQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ICollection<StudentReportForList>> Handle(GetStudentReportListQuery request, CancellationToken cancellationToken)
    {
        return await _context.StudentReports
            .Where(report => report.StudentId == request.StudentId)
            .ProjectTo<StudentReportForList>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}

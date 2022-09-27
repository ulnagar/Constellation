namespace Constellation.Application.Features.Portal.School.Reports.Queries;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetFileFromDatabaseQuery : IRequest<StoredFile>
{
    public string LinkType { get; set; }
    public string LinkId { get; set; }
}

public class GetFileFromDatabaseQueryHandler : IRequestHandler<GetFileFromDatabaseQuery, StoredFile>
{
    private readonly IAppDbContext _context;

    public GetFileFromDatabaseQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile> Handle(GetFileFromDatabaseQuery request, CancellationToken cancellationToken)
    {
        return await _context.StoredFiles
            .SingleOrDefaultAsync(file => file.LinkType == request.LinkType && file.LinkId == request.LinkId, cancellationToken);
    }
}
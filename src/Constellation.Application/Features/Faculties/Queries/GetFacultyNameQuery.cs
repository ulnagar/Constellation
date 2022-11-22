namespace Constellation.Application.Features.Faculties.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetFacultyNameQuery(Guid FacultyId) : IRequest<string> { }

public class GetFacultyNameQueryHandler : IRequestHandler<GetFacultyNameQuery, string>
{
    private readonly IAppDbContext _context;

    public GetFacultyNameQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetFacultyNameQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => faculty.Id == request.FacultyId)
            .Select(faculty => faculty.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

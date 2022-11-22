namespace Constellation.Application.Features.Faculties.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetDictionaryOfFacultiesQuery : IRequest<IDictionary<Guid, string>>
{
}

public class GetListOfFaculiesQueryHandler : IRequestHandler<GetDictionaryOfFacultiesQuery, IDictionary<Guid, string>>
{
    private readonly IAppDbContext _context;

    public GetListOfFaculiesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IDictionary<Guid, string>> Handle(GetDictionaryOfFacultiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => !faculty.IsDeleted)
            .ToDictionaryAsync(faculty => faculty.Id, faculty => faculty.Name, cancellationToken);
    }
}

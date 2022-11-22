namespace Constellation.Application.Features.Faculties.Queries;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetListOfFacultyManagersQuery : IRequest<List<Staff>>
{
    public Guid FacultyId { get; init; }
}

public class GetListOfFacultyManagersQueryHandler : IRequestHandler<GetListOfFacultyManagersQuery, List<Staff>>
{
    private readonly IAppDbContext _context;

    public GetListOfFacultyManagersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Staff>> Handle(GetListOfFacultyManagersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => faculty.Id == request.FacultyId)
            .SelectMany(faculty => faculty.Members)
            .Where(member => member.Role == Core.Enums.FacultyMembershipRole.Manager && !member.IsDeleted)
            .Select(member => member.Staff)
            .ToListAsync(cancellationToken);
    }
}

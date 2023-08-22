using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Features.Home.Queries;

public record GetUsersClassesQuery : IRequest<Dictionary<string, OfferingId>>
{
    public string Username { get; set; }
}

public class GetUsersClassesQueryHandler : IRequestHandler<GetUsersClassesQuery, Dictionary<string, OfferingId>>
{
    private readonly IAppDbContext _context;

    public GetUsersClassesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, OfferingId>> Handle(GetUsersClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = new Dictionary<string, OfferingId>();

        var teacher = await _context.Staff
                .FirstOrDefaultAsync(member => request.Username.Contains(member.PortalUsername));

        if (teacher != null)
        {
            var entries = await _context.Offerings
                .OrderBy(o => o.Name)
                .ThenBy(o => o.StartDate)
                .Where(o => o.Sessions.Any(s => s.StaffId == teacher.StaffId && !s.IsDeleted && s.Period.Type != "Other"))
                .Where(o => o.StartDate < DateTime.Now && o.EndDate > DateTime.Now)
                .ToListAsync();

            foreach (var entry in entries)
            {
                classes.Add(entry.Name, entry.Id);
            }
        }

        return classes;
    }
}

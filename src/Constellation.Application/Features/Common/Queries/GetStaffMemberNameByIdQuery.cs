namespace Constellation.Application.Features.Common.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetStaffMemberNameByIdQuery : IRequest<string>
{
    public string StaffId { get; init; }
}

public class GetStaffMemberNameByIdQueryHandler : IRequestHandler<GetStaffMemberNameByIdQuery, string>
{
    private readonly IAppDbContext _context;

    public GetStaffMemberNameByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetStaffMemberNameByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Staff
            .Where(staff => staff.StaffId == request.StaffId)
            .Select(staff => staff.DisplayName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

namespace Constellation.Application.Features.Common.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetStaffMembersAsDictionaryQuery : IRequest<Dictionary<string, string>>
{
}

public class GetStaffMembersAsDictionaryQueryHandler : IRequestHandler<GetStaffMembersAsDictionaryQuery, Dictionary<string, string>>
{
	private readonly IAppDbContext _context;

	public GetStaffMembersAsDictionaryQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<Dictionary<string, string>> Handle(GetStaffMembersAsDictionaryQuery request, CancellationToken cancellationToken)
	{
		return await _context.Staff
			.Where(staff => !staff.IsDeleted)
			.OrderBy(staff => staff.LastName)
			.ToDictionaryAsync(staff => staff.StaffId, staff => $"{staff.FirstName} {staff.LastName}", cancellationToken);
	}
}

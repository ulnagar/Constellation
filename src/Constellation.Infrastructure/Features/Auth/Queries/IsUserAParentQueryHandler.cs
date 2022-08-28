namespace Constellation.Infrastructure.Features.Auth.Queries;

using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class IsUserAParentQueryHandler : IRequestHandler<IsUserAParentQuery, bool>
{
	private readonly IAppDbContext _context;

	public IsUserAParentQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<bool> Handle(IsUserAParentQuery request, CancellationToken cancellationToken)
	{
		return await _context.StudentFamilies
			.AnyAsync(family => family.Parent1.EmailAddress == request.EmailAddress || family.Parent2.EmailAddress == request.EmailAddress);
	}
}

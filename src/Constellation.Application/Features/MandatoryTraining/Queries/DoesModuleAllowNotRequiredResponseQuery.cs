namespace Constellation.Application.Features.MandatoryTraining.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record DoesModuleAllowNotRequiredResponseQuery(Guid ModuleId) : IRequest<bool>
{ }

public class DoesModuleAllowNotRequiredResponseQueryHandler : IRequestHandler<DoesModuleAllowNotRequiredResponseQuery, bool>
{
    private readonly IAppDbContext _context;

    public DoesModuleAllowNotRequiredResponseQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DoesModuleAllowNotRequiredResponseQuery request, CancellationToken cancellationToken)
    {
        return await _context.MandatoryTraining.Modules
            .Where(module => module.Id == request.ModuleId)
            .Select(module => module.CanMarkNotRequired)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

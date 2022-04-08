using Constellation.Application.Features.Admin.HangfireDashboard.Commands;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Admin.HangfireDashboard.Commands
{
    public class ToggleJobActivationCommandHandler : IRequestHandler<ToggleJobActivationCommand>
    {
        private readonly IAppDbContext _context;

        public ToggleJobActivationCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ToggleJobActivationCommand request, CancellationToken cancellationToken)
        {
            var record = await _context.JobActivations.SingleOrDefaultAsync(job => job.Id == request.Id, cancellationToken);

            if (record == null) return Unit.Value;

            record.IsActive = !record.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

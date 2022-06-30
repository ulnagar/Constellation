using Constellation.Application.Features.Portal.School.Stocktake.Commands;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Stocktake.Commands
{
    public class CancelStocktakeSightingCommandHandler : IRequestHandler<CancelStocktakeSightingCommand>
    {
        private readonly IAppDbContext _context;

        public CancelStocktakeSightingCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CancelStocktakeSightingCommand request, CancellationToken cancellationToken)
        {
            var sighting = await _context.StocktakeSightings
                .FirstOrDefaultAsync(sighting => sighting.Id == request.SightingId, cancellationToken);

            if (sighting == null)
                return Unit.Value;

            sighting.IsCancelled = true;
            sighting.CancellationComment = request.Comment;
            sighting.CancelledBy = request.CancelledBy;
            sighting.CancelledAt = request.CancelledAt;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

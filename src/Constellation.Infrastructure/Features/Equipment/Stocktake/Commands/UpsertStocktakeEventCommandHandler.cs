using Constellation.Application.Features.Equipment.Stocktake.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Equipment.Stocktake.Commands
{
    public class UpsertStocktakeEventCommandHandler : IRequestHandler<UpsertStocktakeEventCommand>
    {
        private readonly IAppDbContext _context;

        public UpsertStocktakeEventCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpsertStocktakeEventCommand request, CancellationToken cancellationToken)
        {
            var existingEvent = await _context.StocktakeEvents
                .FirstOrDefaultAsync(stocktake => stocktake.Id == request.Id, cancellationToken);

            if (existingEvent != null)
            {
                existingEvent.StartDate = request.StartDate;
                existingEvent.EndDate = request.EndDate;
                existingEvent.Name = request.Name;
                existingEvent.AcceptLateResponses = request.AcceptLateResponses;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }

            var stocktake = new StocktakeEvent
            {
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AcceptLateResponses = request.AcceptLateResponses
            };

            _context.Add(stocktake);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

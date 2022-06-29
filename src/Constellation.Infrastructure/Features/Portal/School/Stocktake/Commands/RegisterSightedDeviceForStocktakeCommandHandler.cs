using Constellation.Application.Features.Portal.School.Stocktake.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Stocktake.Commands
{
    public class RegisterSightedDeviceForStocktakeCommandHandler : IRequestHandler<RegisterSightedDeviceForStocktakeCommand>
    {
        private readonly IAppDbContext _context;

        public RegisterSightedDeviceForStocktakeCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RegisterSightedDeviceForStocktakeCommand request, CancellationToken cancellationToken)
        {
            var sighting = new StocktakeSighting
            {
                StocktakeEventId = request.StocktakeEventId,
                AssetNumber = request.AssetNumber,
                SerialNumber = request.SerialNumber,
                Description = request.Description,
                LocationCategory = request.LocationCategory,
                LocationName = request.LocationName,
                LocationCode = request.LocationCode,
                UserType = request.UserType,
                UserName = request.UserName,
                UserCode = request.UserCode,
                Comment = request.Comment,
                SightedBy = request.SightedBy,
                SightedAt = request.SightedAt
            };

            _context.Add(sighting);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

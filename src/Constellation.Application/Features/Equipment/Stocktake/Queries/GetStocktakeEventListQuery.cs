using Constellation.Core.Models.Stocktake;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Equipment.Stocktake.Queries
{
    public class GetStocktakeEventListQuery : IRequest<ICollection<StocktakeEvent>>
    {
    }
}

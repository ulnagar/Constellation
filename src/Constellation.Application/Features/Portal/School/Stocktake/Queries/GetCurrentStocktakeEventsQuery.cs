using Constellation.Application.Features.Portal.School.Stocktake.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Stocktake.Queries
{
    public class GetCurrentStocktakeEventsQuery : IRequest<ICollection<StocktakeEventsForList>>
    {
    }
}

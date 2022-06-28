using Constellation.Core.Models.Stocktake;
using MediatR;
using System;

namespace Constellation.Application.Features.Equipment.Stocktake.Queries
{
    public class GetStocktakeEventQuery : IRequest<StocktakeEvent>
    {
        public Guid StocktakeId { get; set; }
        public bool IncludeSightings { get; set; } = false;
    }
}

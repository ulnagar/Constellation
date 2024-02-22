namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Core.Models.Stocktake;
using MediatR;
using System;

public sealed class GetStocktakeEventQuery : IRequest<StocktakeEvent>
{
    public Guid StocktakeId { get; set; }
    public bool IncludeSightings { get; set; } = false;
}
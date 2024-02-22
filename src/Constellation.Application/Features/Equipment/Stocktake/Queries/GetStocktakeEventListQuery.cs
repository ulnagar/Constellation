namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Core.Models.Stocktake;
using MediatR;
using System.Collections.Generic;

public sealed class GetStocktakeEventListQuery 
    : IRequest<ICollection<StocktakeEvent>>
{ }
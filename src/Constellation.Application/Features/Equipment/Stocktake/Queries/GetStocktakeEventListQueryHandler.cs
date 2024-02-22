namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeEventListQueryHandler 
    :IRequestHandler<GetStocktakeEventListQuery, ICollection<StocktakeEvent>>
{
    private readonly IStocktakeRepository _stocktakeRepository;

    public GetStocktakeEventListQueryHandler(
        IStocktakeRepository stocktakeRepository)
    {
        _stocktakeRepository = stocktakeRepository;
    }

    public async Task<ICollection<StocktakeEvent>> Handle(GetStocktakeEventListQuery request, CancellationToken cancellationToken)
    {
        List<StocktakeEvent> events = await _stocktakeRepository.GetAll(cancellationToken);

        return events;
    }
}
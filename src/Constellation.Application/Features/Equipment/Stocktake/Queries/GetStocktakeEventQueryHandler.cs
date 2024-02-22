namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeEventQueryHandler 
    : IRequestHandler<GetStocktakeEventQuery, StocktakeEvent>
{
    private readonly IStocktakeRepository _stocktakeRepository;

    public GetStocktakeEventQueryHandler(
        IStocktakeRepository stocktakeRepository)
    {
        _stocktakeRepository = stocktakeRepository;
    }

    public async Task<StocktakeEvent> Handle(GetStocktakeEventQuery request, CancellationToken cancellationToken) =>
        request.IncludeSightings switch
        {
            true => await _stocktakeRepository.GetByIdWithSightings(request.StocktakeId, cancellationToken),
            false => await _stocktakeRepository.GetById(request.StocktakeId, cancellationToken)
        };
}
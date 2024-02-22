namespace Constellation.Application.Features.Equipment.Stocktake.Commands;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertStocktakeEventCommandHandler 
    : IRequestHandler<UpsertStocktakeEventCommand>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertStocktakeEventCommandHandler(
        IStocktakeRepository stocktakeRepository,
        IUnitOfWork unitOfWork)
    {
        _stocktakeRepository = stocktakeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpsertStocktakeEventCommand request, CancellationToken cancellationToken)
    {
        StocktakeEvent existingEvent = await _stocktakeRepository.GetById(request.Id, cancellationToken); 

        if (existingEvent is not null)
        {
            existingEvent.StartDate = request.StartDate;
            existingEvent.EndDate = request.EndDate;
            existingEvent.Name = request.Name;
            existingEvent.AcceptLateResponses = request.AcceptLateResponses;

            await _unitOfWork.CompleteAsync(cancellationToken);

            return Unit.Value;
        }

        StocktakeEvent stocktake = new()
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AcceptLateResponses = request.AcceptLateResponses
        };

        _stocktakeRepository.Insert(stocktake);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Unit.Value;
    }
}
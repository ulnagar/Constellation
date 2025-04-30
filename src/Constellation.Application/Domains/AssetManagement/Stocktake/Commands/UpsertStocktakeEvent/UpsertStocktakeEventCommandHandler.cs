namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.UpsertStocktakeEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Stocktake.Repositories;
using Core.Models.Stocktake.Errors;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertStocktakeEventCommandHandler
    :ICommandHandler<UpsertStocktakeEventCommand>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertStocktakeEventCommandHandler(
        IStocktakeRepository stocktakeRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpsertStocktakeEventCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            StocktakeEvent existingEvent = await _stocktakeRepository.GetById(request.Id.Value, cancellationToken);

            if (existingEvent is null)
            {
                _logger
                    .ForContext(nameof(UpsertStocktakeEventCommand), request, true)
                    .ForContext(nameof(Error), StocktakeErrors.EventNotFound(request.Id.Value), true)
                    .Warning("Failed to update Stocktake Event");

                return Result.Failure(StocktakeErrors.EventNotFound(request.Id.Value));
            }

            existingEvent.StartDate = request.StartDate;
            existingEvent.EndDate = request.EndDate;
            existingEvent.Name = request.Name;
            existingEvent.AcceptLateResponses = request.AcceptLateResponses;
        }
        else
        {
            StocktakeEvent stocktake = new()
            {
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AcceptLateResponses = request.AcceptLateResponses
            };

            _stocktakeRepository.Insert(stocktake);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

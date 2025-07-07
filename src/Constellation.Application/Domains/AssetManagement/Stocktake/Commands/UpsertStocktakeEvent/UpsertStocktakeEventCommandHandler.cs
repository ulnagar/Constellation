namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.UpsertStocktakeEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Stocktake.Repositories;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Identifiers;
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
        if (!request.Id.Equals(StocktakeEventId.Empty))
        {
            StocktakeEvent existingEvent = await _stocktakeRepository.GetById(request.Id, cancellationToken);

            if (existingEvent is null)
            {
                _logger
                    .ForContext(nameof(UpsertStocktakeEventCommand), request, true)
                    .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.Id), true)
                    .Warning("Failed to update Stocktake Event");

                return Result.Failure(StocktakeEventErrors.EventNotFound(request.Id));
            }

            Result update = existingEvent.Update(
                request.Name,
                request.StartDate,
                request.EndDate,
                request.AcceptLateResponses);

            if (update.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpsertStocktakeEventCommand), request, true)
                    .ForContext(nameof(Error), update.Error, true)
                    .Warning("Failed to update Stocktake Event");

                return Result.Failure(update.Error);
            }
        }
        else
        {
            StocktakeEvent stocktake = new(
                request.Name,
                request.StartDate,
                request.EndDate,
                request.AcceptLateResponses);

            _stocktakeRepository.Insert(stocktake);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

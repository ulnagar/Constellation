namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.CancelSighting;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CancelSightingCommandHandler 
    : ICommandHandler<CancelSightingCommand>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IUnitOfWork _uniOfWork;
    private readonly ILogger _logger;

    public CancelSightingCommandHandler(
        IStocktakeRepository stocktakeRepository,
        IUnitOfWork uniOfWork,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _uniOfWork = uniOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelSightingCommand request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.EventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(CancelSightingCommand), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.EventId), true)
                .Warning("Failed to cancel Stocktake Sighting");

            return Result.Failure(StocktakeEventErrors.EventNotFound(request.EventId));
        }

        Result cancel = @event.CancelSighting(
            request.SightingId,
            request.Comment,
            request.CancelledBy);

        if (cancel.IsFailure)
        {
            _logger
                .ForContext(nameof(CancelSightingCommand), request, true)
                .ForContext(nameof(Error), cancel.Error, true)
                .Warning("Failed to cancel Stocktake Sighting");

            return Result.Failure(cancel.Error);
        }

        await _uniOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSighting;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterSightingCommandHandler 
    : ICommandHandler<RegisterSightingCommand>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RegisterSightingCommandHandler(
        IStocktakeRepository stocktakeRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RegisterSightingCommand>();
    }

    public async Task<Result> Handle(RegisterSightingCommand request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.StocktakeEventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(RegisterSightingCommand), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.StocktakeEventId), true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(StocktakeEventErrors.EventNotFound(request.StocktakeEventId));
        }

        Result sighting = @event.AddSighting(
            request.SerialNumber,
            request.AssetNumber,
            request.Description,
            request.LocationCategory,
            request.LocationName,
            request.LocationCode,
            request.UserType,
            request.UserName,
            request.UserCode,
            request.Comment,
            request.SightedBy,
            request.SightedAt);

        if (sighting.IsFailure)
        {
            _logger
                .ForContext(nameof(RegisterSightingCommand), request, true)
                .ForContext(nameof(Error), sighting.Error, true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(sighting.Error);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
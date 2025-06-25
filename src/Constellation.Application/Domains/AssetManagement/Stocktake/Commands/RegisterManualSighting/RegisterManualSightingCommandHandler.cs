namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterManualSighting;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Constellation.Core.Models.Stocktake.Errors;
using Constellation.Core.Models.Stocktake.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Enums;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RegisterManualSightingCommandHandler 
    : ICommandHandler<RegisterManualSightingCommand>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RegisterManualSightingCommandHandler(
        IStocktakeRepository stocktakeRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RegisterManualSightingCommand>();
    }

    public async Task<Result> Handle(RegisterManualSightingCommand request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.StocktakeEventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(RegisterManualSightingCommand), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.StocktakeEventId), true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(StocktakeEventErrors.EventNotFound(request.StocktakeEventId));
        }

        Result sighting = @event.AddSighting(
            request.SerialNumber,
            AssetNumber.Empty, 
            request.Description,
            request.LocationCategory,
            request.LocationName,
            request.LocationCode,
            request.UserType,
            request.UserName,
            request.UserCode,
            request.Comment,
            _currentUserService.UserName,
            _dateTime.Now,
            DifferenceCategory.ManualEntry);

        if (sighting.IsFailure)
        {
            _logger
                .ForContext(nameof(RegisterManualSightingCommand), request, true)
                .ForContext(nameof(Error), sighting.Error, true)
                .Warning("Failed to create new Stocktake Sighting record");

            return Result.Failure(sighting.Error);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
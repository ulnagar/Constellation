namespace Constellation.Application.Offerings.RemoveResourceFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveResourceFromOfferingCommandHandler
    : ICommandHandler<RemoveResourceFromOfferingCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveResourceFromOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveResourceFromOfferingCommand>();
    }

    public async Task<Result> Handle(RemoveResourceFromOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find Offering with Id {id}", request.OfferingId);

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        Result<Resource> action = offering.RemoveResource(request.ResourceId);

        if (action.IsSuccess)
        {
            _logger.Warning(action.Error.Message);

            return Result.Failure(action.Error);
        }

        _offeringRepository.Remove(action.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

namespace Constellation.Application.Offerings.AddResourceToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models.Offerings.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddResourceToOfferingCommandHandler
    : ICommandHandler<AddResourceToOfferingCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddResourceToOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddResourceToOfferingCommand>();
    }

    public async Task<Result> Handle(AddResourceToOfferingCommand request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("Could not find Offering with Id {id}", request.OfferingId);

            return Result.Failure(OfferingErrors.NotFound(request.OfferingId));
        }

        Result attempt;

        if (request.ResourceType.Equals(ResourceType.CanvasCourse))
        {
            string year = offering.EndDate.Year.ToString();
            string code = offering.Name;

            if (request.ResourceId.Equals($"{year}-{code[..^2]}"))
                attempt = offering.AddResource(request.ResourceType, request.ResourceId, request.Name, request.Url, $"{year}-{code}");
            else
                attempt = offering.AddResource(request.ResourceType, request.ResourceId, request.Name, request.Url);
        }
        else
        {
            attempt = offering.AddResource(request.ResourceType, request.ResourceId, request.Name, request.Url);
        }

        if (attempt.IsFailure)
            return attempt;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

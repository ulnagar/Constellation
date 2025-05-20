namespace Constellation.Application.Domains.Edval.Events.EdvalClassesUpdated;

using Application.Abstractions.Messaging;
using Constellation.Core.Models.Edval.Events;
using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalClassesUpdatedIntegrationEvent>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CalculateDifferences>();
    }

    public async Task Handle(EdvalClassesUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Offering> existingClasses = await _offeringRepository.GetAllActive(cancellationToken);

        List<EdvalClass> edvalClasses = await _edvalRepository.GetClasses(cancellationToken);

        List<EdvalIgnore> ignoredClasses = await _edvalRepository.GetIgnoreRecords(EdvalDifferenceType.EdvalClass, cancellationToken);

        foreach (EdvalClass edvalClass in edvalClasses)
        {
            bool ignored = ignoredClasses
                .Where(ignore => ignore.System == EdvalDifferenceSystem.EdvalDifference)
                .Any(ignore => ignore.Identifier == edvalClass.EdvalClassCode);

            if (existingClasses.All(entry => entry.Name.Value != edvalClass.OfferingName))
            {
                // Additional class in Edval
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalClass,
                    EdvalDifferenceSystem.EdvalDifference, 
                    edvalClass.EdvalClassCode,
                    $"{edvalClass.OfferingName} is not present in Constellation",
                    ignored));
            }
        }

        foreach (Offering existingClass in existingClasses)
        {
            bool ignored = ignoredClasses
                .Where(ignore => ignore.System == EdvalDifferenceSystem.ConstellationDifference)
                .Any(ignore => ignore.Identifier == existingClass.Id.ToString());

            if (edvalClasses.All(entry => entry.OfferingName != existingClass.Name.Value))
            {
                // Additional class in Constellation
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalClass,
                    EdvalDifferenceSystem.ConstellationDifference,
                    existingClass.Id.ToString(),
                    $"{existingClass.Name} is not present in Edval",
                    ignored));
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
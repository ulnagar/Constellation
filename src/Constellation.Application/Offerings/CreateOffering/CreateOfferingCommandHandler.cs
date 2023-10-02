namespace Constellation.Application.Offerings.CreateOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOfferingCommandHandler
    : ICommandHandler<CreateOfferingCommand, OfferingId>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateOfferingCommandHandler(
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateOfferingCommand>();
    }

    public async Task<Result<OfferingId>> Handle(CreateOfferingCommand request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            _logger
                .ForContext(nameof(CreateOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.Validation.StartDateAfterEndDate, true)
                .Warning("Failed to create Offering");

            return Result.Failure<OfferingId>(OfferingErrors.Validation.StartDateAfterEndDate);
        }

        if (request.EndDate < _dateTime.Today)
        {
            _logger
                .ForContext(nameof(CreateOfferingCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.Validation.EndDateInPast, true)
                .Warning("Failed to create Offering");

            return Result.Failure<OfferingId>(OfferingErrors.Validation.EndDateInPast);
        }

        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(CreateOfferingCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to create Offering");

            return Result.Failure<OfferingId>(CourseErrors.NotFound(request.CourseId));
        }

        Result<OfferingName> name = OfferingName.FromValue(request.Name);

        if (name is null)
        {
            _logger
                .ForContext(nameof(CreateOfferingCommand), request, true)
                .ForContext(nameof(Error), name.Error, true)
                .Warning("Failed to create Offering");

            return Result.Failure<OfferingId>(name.Error);
        }

        Offering offering = new Offering(
            name.Value, 
            course.Id, 
            request.StartDate, 
            request.EndDate);

        _offeringRepository.Insert(offering);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return offering.Id;
    }
}

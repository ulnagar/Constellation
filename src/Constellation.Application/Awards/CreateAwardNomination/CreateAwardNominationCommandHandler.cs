namespace Constellation.Application.Awards.CreateAwardNomination;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAwardNominationCommandHandler
    : ICommandHandler<CreateAwardNominationCommand>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAwardNominationCommandHandler(
        IAwardNominationRepository nominationRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateAwardNominationCommand>();
    }

    public async Task<Result> Handle(CreateAwardNominationCommand request, CancellationToken cancellationToken)
    {
        NominationPeriod period = await _nominationRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .Warning("Could not find Award Nomination Period when attempting to create nomination");

            return Result.Failure(DomainErrors.Awards.NominationPeriod.NotFound(request.PeriodId));
        }

        Nomination nomination = null;

        if (request.AwardType.Equals(AwardType.FirstInSubject))
        {
            Course course = await _courseRepository.GetById(request.CourseId);

            nomination = new FirstInSubjectNomination(request.PeriodId, request.StudentId, request.CourseId, course.Name);
        }

        if (request.AwardType.Equals(AwardType.AcademicExcellence))
        {
            Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

            Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

            nomination = new AcademicExcellenceNomination(request.PeriodId, request.StudentId, request.CourseId, course.Name, request.OfferingId, offering.Name);
        }

        if (request.AwardType.Equals(AwardType.AcademicAchievement))
        {
            Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

            Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

            nomination = new AcademicAchievementNomination(request.PeriodId, request.StudentId, request.CourseId, course.Name, request.OfferingId, offering.Name);
        }

        if (request.AwardType.Equals(AwardType.PrincipalsAward))
        {
            nomination = new PrincipalsAwardNomination(request.PeriodId, request.StudentId);
        }

        if (request.AwardType.Equals(AwardType.GalaxyMedal))
        {
            nomination = new GalaxyMedalNomination(request.PeriodId, request.StudentId);
        }

        if (request.AwardType.Equals(AwardType.UniversalAchiever))
        {
            nomination = new UniversalAchieverNomination(request.PeriodId, request.StudentId);
        }

        if (nomination is null)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .Warning("Unknown award type: could not be created");

            return Result.Failure(DomainErrors.Awards.Nomination.NotRecognised);
        }

        period.AddNomination(nomination);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

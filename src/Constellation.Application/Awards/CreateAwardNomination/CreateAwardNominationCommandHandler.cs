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
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Serilog;
using System;
using System.Linq;
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
        
        Course? course = request.CourseId != CourseId.Empty ? await _courseRepository.GetById(request.CourseId, cancellationToken) : null;
        Offering? offering = request.OfferingId != OfferingId.Empty ? await _offeringRepository.GetById(request.OfferingId, cancellationToken) : null;

        if (course is null)
        {
            if (request.AwardType != AwardType.PrincipalsAward &&
                request.AwardType != AwardType.GalaxyMedal &&
                request.AwardType != AwardType.UniversalAchiever)
                return Result.Failure(DomainErrors.Awards.Nomination.InvalidCourseId);
        }

        if (offering is null)
        {
            if (request.AwardType != AwardType.FirstInSubject &&
                request.AwardType != AwardType.PrincipalsAward &&
                request.AwardType != AwardType.GalaxyMedal &&
                request.AwardType != AwardType.UniversalAchiever)
                return Result.Failure(DomainErrors.Awards.Nomination.InvalidOfferingId);
        }

        Nomination? nomination = request.AwardType switch
        {
            _ when request.AwardType == AwardType.FirstInSubject => new FirstInSubjectNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Grade, course.Name),
            _ when request.AwardType == AwardType.AcademicExcellence => new AcademicExcellenceNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicExcellenceMathematics => new AcademicExcellenceMathematicsNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicExcellenceScienceTechnology => new AcademicExcellenceScienceTechnologyNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicAchievement => new AcademicAchievementNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.AcademicAchievementMathematics => new AcademicAchievementMathematicsNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.AcademicAchievementScienceTechnology => new AcademicAchievementScienceTechnologyNomination(request.PeriodId, request.StudentId, request.CourseId, course!.Name, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.PrincipalsAward => new PrincipalsAwardNomination(request.PeriodId, request.StudentId),
            _ when request.AwardType == AwardType.GalaxyMedal => new GalaxyMedalNomination(request.PeriodId, request.StudentId),
            _ when request.AwardType == AwardType.UniversalAchiever => new UniversalAchieverNomination(request.PeriodId, request.StudentId),
            _ => null
        };
        
        if (nomination is null)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .Warning("Unknown award type: could not be created");

            return Result.Failure(DomainErrors.Awards.Nomination.NotRecognised);
        }

        // Check that there is not a duplicate nomination already existing in the period
        if (HasDuplicateEntry(period, nomination))
        {
            _logger
                .ForContext(nameof(request), request, true)
                .ForContext(nameof(Nomination), nomination, true)
                .ForContext(nameof(Error), DomainErrors.Awards.Nomination.DuplicateFound, true)
                .Warning("Award nomination already exists");

            return Result.Failure(DomainErrors.Awards.Nomination.DuplicateFound);
        }
        
        period.AddNomination(nomination);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    private static bool HasDuplicateEntry(NominationPeriod period, Nomination newAward) =>
        newAward switch
        {
            FirstInSubjectNomination fis => 
                period.Nominations
                    .OfType<FirstInSubjectNomination>()
                    .Any(entry =>
                        entry.StudentId == fis.StudentId &&
                        entry.CourseId == fis.CourseId &&
                        !entry.IsDeleted),

            AcademicExcellenceNomination ae => 
                period.Nominations
                    .OfType<AcademicExcellenceNomination>()
                    .Any(entry =>
                        entry.StudentId == ae.StudentId &&
                        entry.CourseId == ae.CourseId &&
                        entry.OfferingId == ae.OfferingId &&
                        !entry.IsDeleted),

            AcademicExcellenceMathematicsNomination aem => 
                period.Nominations
                    .OfType<AcademicExcellenceMathematicsNomination>()
                    .Any(entry => 
                        entry.StudentId == aem.StudentId &&
                        entry.CourseId == aem.CourseId &&
                        entry.OfferingId == aem.OfferingId &&
                        !entry.IsDeleted),

            AcademicExcellenceScienceTechnologyNomination aest => 
                period.Nominations
                    .OfType<AcademicExcellenceScienceTechnologyNomination>()
                    .Any(entry =>
                        entry.StudentId == aest.StudentId &&
                        entry.CourseId == aest.CourseId &&
                        entry.OfferingId == aest.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementNomination aa => 
                period.Nominations
                    .OfType<AcademicAchievementNomination>()
                    .Any(entry =>
                        entry.StudentId == aa.StudentId &&
                        entry.CourseId == aa.CourseId &&
                        entry.OfferingId == aa.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementMathematicsNomination aam =>
                period.Nominations
                    .OfType<AcademicAchievementMathematicsNomination>()
                    .Any(entry =>
                        entry.StudentId == aam.StudentId &&
                        entry.CourseId == aam.CourseId &&
                        entry.OfferingId == aam.OfferingId &&
                        !entry.IsDeleted),

            AcademicAchievementScienceTechnologyNomination aast =>
                period.Nominations
                    .OfType<AcademicAchievementScienceTechnologyNomination>()
                    .Any(entry =>
                        entry.StudentId == aast.StudentId &&
                        entry.CourseId == aast.CourseId &&
                        entry.OfferingId == aast.OfferingId &&
                        !entry.IsDeleted),

            PrincipalsAwardNomination pa => 
                period.Nominations
                    .OfType<PrincipalsAwardNomination>()
                    .Any(entry =>
                        entry.StudentId == pa.StudentId &&
                        !entry.IsDeleted),

            GalaxyMedalNomination ga => 
                period.Nominations
                    .OfType<GalaxyMedalNomination>()
                    .Any(entry =>
                        entry.StudentId == ga.StudentId &&
                        !entry.IsDeleted),

            UniversalAchieverNomination ua =>
                period.Nominations
                    .OfType<UniversalAchieverNomination>()
                    .Any(entry =>
                        entry.StudentId == ua.StudentId &&
                        !entry.IsDeleted),

            _ => throw new NotImplementedException()
        };
}

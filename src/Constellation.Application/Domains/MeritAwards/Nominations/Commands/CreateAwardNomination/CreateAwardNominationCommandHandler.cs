#nullable enable
using Constellation;

namespace Constellation.Application.Domains.MeritAwards.Nominations.Commands.CreateAwardNomination;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Core.Errors;
using Core.Models.Awards.Errors;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateAwardNominationCommandHandler
    : ICommandHandler<CreateAwardNominationCommand>
{
    private readonly IAwardNominationRepository _nominationRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAwardNominationCommandHandler(
        IAwardNominationRepository nominationRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _nominationRepository = nominationRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
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

            return Result.Failure(AwardNominationPeriodErrors.NotFound(request.PeriodId));
        }

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .Warning("Could not find Student when attempting to create nomination");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        if (student.CurrentEnrolment is null)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .Warning("Could not find Student enrolment when attempting to create nomination");

            return Result.Failure(SchoolEnrolmentErrors.NotFound);
        }

        Course? course = request.CourseId != CourseId.Empty ? await _courseRepository.GetById(request.CourseId, cancellationToken) : null;
        Offering? offering = request.OfferingId != OfferingId.Empty ? await _offeringRepository.GetById(request.OfferingId, cancellationToken) : null;

        if (course is null)
        {
            if (request.AwardType != AwardType.PrincipalsAward &&
                request.AwardType != AwardType.GalaxyMedal &&
                request.AwardType != AwardType.UniversalAchiever)
                return Result.Failure(AwardNominationErrors.InvalidCourseId);
        }

        if (offering is null)
        {
            if (request.AwardType != AwardType.FirstInSubject &&
                request.AwardType != AwardType.FirstInSubjectMathematics &&
                request.AwardType != AwardType.FirstInSubjectScienceTechnology &&
                request.AwardType != AwardType.PrincipalsAward &&
                request.AwardType != AwardType.GalaxyMedal &&
                request.AwardType != AwardType.UniversalAchiever)
                return Result.Failure(AwardNominationErrors.InvalidOfferingId);
        }

        Result<Nomination> nomination = request.AwardType switch
        {
            _ when request.AwardType == AwardType.FirstInSubject => FirstInSubjectNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Grade, course.Name),
            _ when request.AwardType == AwardType.FirstInSubjectMathematics => FirstInSubjectMathematicsNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Grade, course.Name),
            _ when request.AwardType == AwardType.FirstInSubjectScienceTechnology => FirstInSubjectScienceTechnologyNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Grade, course.Name),
            _ when request.AwardType == AwardType.AcademicExcellence => AcademicExcellenceNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicExcellenceMathematics => AcademicExcellenceMathematicsNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicExcellenceScienceTechnology => AcademicExcellenceScienceTechnologyNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name),
            _ when request.AwardType == AwardType.AcademicAchievement => AcademicAchievementNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.AcademicAchievementMathematics => AcademicAchievementMathematicsNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.AcademicAchievementScienceTechnology => AcademicAchievementScienceTechnologyNomination.Create(request.PeriodId, request.StudentId, request.CourseId, course!.Name, course!.Grade, request.OfferingId, offering!.Name.Value),
            _ when request.AwardType == AwardType.PrincipalsAward => PrincipalsAwardNomination.Create(request.PeriodId, request.StudentId, student.CurrentEnrolment!.Grade),
            _ when request.AwardType == AwardType.GalaxyMedal => GalaxyMedalNomination.Create(request.PeriodId, request.StudentId, student.CurrentEnrolment!.Grade),
            _ when request.AwardType == AwardType.UniversalAchiever => UniversalAchieverNomination.Create(request.PeriodId, request.StudentId, student.CurrentEnrolment!.Grade),
            _ => Result.Failure<Nomination>(AwardNominationErrors.NotRecognised)
        };
        
        if (nomination.IsFailure)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .ForContext(nameof(Error), nomination.Error, true)
                .Warning("Failed to create new award");

            return Result.Failure(nomination.Error);
        }

        Result addNomination = period.AddNomination(nomination.Value);

        if (addNomination.IsFailure)
        {
            _logger
                .ForContext(nameof(request), request, true)
                .ForContext(nameof(Nomination), nomination, true)
                .ForContext(nameof(Error), addNomination.Error, true)
                .Warning("Award nomination already exists");

            return Result.Failure(addNomination.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

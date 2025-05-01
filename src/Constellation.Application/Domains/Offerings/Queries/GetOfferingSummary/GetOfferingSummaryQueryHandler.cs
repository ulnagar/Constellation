namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingSummaryQueryHandler
    : IQueryHandler<GetOfferingSummaryQuery, OfferingSummaryResponse>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetOfferingSummaryQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IPeriodRepository periodRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _periodRepository = periodRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetOfferingSummaryQuery>();
    }

    public async Task<Result<OfferingSummaryResponse>> Handle(GetOfferingSummaryQuery request, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(GetOfferingSummaryQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Could not retrieve Offering summary");

            return Result.Failure<OfferingSummaryResponse>(OfferingErrors.NotFound(request.OfferingId));
        }

        List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

        List<string> teacherNames = teachers.Select(teacher => teacher.DisplayName).ToList();

        // Calculate minPerFn
        List<PeriodId> periodIds = offering.Sessions
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .ToList();

        double minPerFn = await _periodRepository.TotalDurationForCollectionOfPeriods(periodIds, cancellationToken);

        Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(GetOfferingSummaryQuery), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(offering.CourseId), true)
                .Warning("Could not retrieve Offering summary");

            return Result.Failure<OfferingSummaryResponse>(CourseErrors.NotFound(offering.CourseId));
        }

        // Get Faculty
        Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetOfferingSummaryQuery), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(course.FacultyId), true)
                .Warning("Could not retrieve Offering summary");

            return Result.Failure<OfferingSummaryResponse>(FacultyErrors.NotFound(course.FacultyId));
        }

        OfferingSummaryResponse entry = new(
            offering.Id,
            offering.Name,
            course.Name,
            offering.StartDate,
            offering.EndDate,
            teacherNames,
            (int)minPerFn,
            faculty.Name,
            course.Grade,
            offering.IsCurrent);

        return entry;
    }
}

namespace Constellation.Application.Domains.Offerings.Queries.GetAllOfferingSummaries;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Offerings.Models;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.Faculties;
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

internal sealed class GetAllOfferingSummariesQueryHandler
    : IQueryHandler<GetAllOfferingSummariesQuery, List<OfferingSummaryResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetAllOfferingSummariesQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetAllOfferingSummariesQuery>();
    }

    public async Task<Result<List<OfferingSummaryResponse>>> Handle(GetAllOfferingSummariesQuery request, CancellationToken cancellationToken)
    {
        List<OfferingSummaryResponse> response = new();

        List<Offering> offerings = request.Filter switch
        {
            GetAllOfferingSummariesQuery.FilterEnum.All => await _offeringRepository.GetAll(cancellationToken),
            GetAllOfferingSummariesQuery.FilterEnum.Active => await _offeringRepository.GetAllActive(cancellationToken),
            GetAllOfferingSummariesQuery.FilterEnum.Future => await _offeringRepository.GetAllFuture(cancellationToken),
            GetAllOfferingSummariesQuery.FilterEnum.Inactive => await _offeringRepository.GetAllInactive(cancellationToken),
            _ => new List<Offering>()
        };

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        foreach (Offering offering in offerings)
        {
            // Get Teachers
            List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

            List<string> teacherNames = teachers.Select(teacher => teacher.DisplayName).ToList();

            // Calculate minPerFn
            List<PeriodId> periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            double minPerFn = await _periodRepository.TotalDurationForCollectionOfPeriods(periodIds, cancellationToken);

            Course course = courses.FirstOrDefault(course => course.Id == offering.CourseId);

            if (course is null)
            {
                _logger
                    .ForContext(nameof(Offering), offering, true)
                    .Warning("Could not find Offering with Id {id}", offering.CourseId);

                continue;
            }

            // Get Faculty
            Faculty faculty = faculties.FirstOrDefault(faculty => faculty.Id == course.FacultyId);

            if (faculty is null)
            {
                _logger
                    .ForContext(nameof(Course), course, true)
                    .Warning("Could not find Faculty with Id {id}", course.FacultyId);

                continue;
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

            response.Add(entry);
        }

        return response;
    }
}

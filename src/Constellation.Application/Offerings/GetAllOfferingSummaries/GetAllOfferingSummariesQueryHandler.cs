namespace Constellation.Application.Offerings.GetAllOfferingSummaries;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
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
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetAllOfferingSummariesQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ITimetablePeriodRepository periodRepository,
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

        List<Offering> offerings = await _offeringRepository.GetAll(cancellationToken);

        foreach (Offering offering in offerings)
        {
            // Get Teachers
            List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

            List<string> teacherNames = teachers.Select(teacher => teacher.DisplayName).ToList();

            // Calculate minPerFn
            List<int> periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            double minPerFn = await _periodRepository.TotalDurationForCollectionOfPeriods(periodIds, cancellationToken);

            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            if (course is null)
            {
                _logger
                    .ForContext(nameof(Offering), offering, true)
                    .Warning("Could not find Offering with Id {id}", offering.CourseId);

                continue;
            }

            // Get Faculty
            Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

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

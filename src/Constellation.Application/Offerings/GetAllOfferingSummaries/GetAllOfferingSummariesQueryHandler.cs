namespace Constellation.Application.Offerings.GetAllOfferingSummaries;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
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
    private readonly IFacultyRepository _facultyRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetAllOfferingSummariesQueryHandler(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ITimetablePeriodRepository periodRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
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
            int minPerFN = (int)offering.Sessions
                .Where(session => !session.IsDeleted)
                .Sum(session => session.Period.EndTime.Subtract(session.Period.StartTime).TotalMinutes);

            // Get Faculty
            Faculty faculty = await _facultyRepository.GetById(offering.Course.FacultyId, cancellationToken);

            if (faculty is null)
            {
                _logger
                    .ForContext(nameof(Offering), offering, true)
                    .Warning("Could not find Faculty with Id {id}", offering.Course.FacultyId);

                continue;
            }

            OfferingSummaryResponse entry = new(
                offering.Id,
                offering.Name,
                offering.Course.Name,
                offering.EndDate,
                teacherNames,
                minPerFN,
                faculty.Name,
                offering.Course.Grade,
                offering.IsCurrent);

            response.Add(entry);
        }

        return response;
    }
}

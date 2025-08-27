namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Covers;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Timetables;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffDailyTimetableDataQueryHandler
: IQueryHandler<GetStaffDailyTimetableDataQuery, List<StaffDailyTimetableResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetStaffDailyTimetableDataQueryHandler(
        IOfferingRepository offeringRepository,
        ICoverRepository coverRepository,
        IPeriodRepository periodRepository,
        ITutorialRepository tutorialRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _coverRepository = coverRepository;
        _periodRepository = periodRepository;
        _tutorialRepository = tutorialRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetStaffDailyTimetableDataQuery>();
    }

    public async Task<Result<List<StaffDailyTimetableResponse>>> Handle(GetStaffDailyTimetableDataQuery request, CancellationToken cancellationToken)
    {
        List<StaffDailyTimetableResponse> response = [];

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);
        List<Tutorial> tutorials = await _tutorialRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        DateOnly today = _dateTime.Today;
        int dayNumber = today.GetDayNumber();
        PeriodWeek week = PeriodWeek.FromDayNumber(dayNumber);
        PeriodDay day = PeriodDay.FromDayNumber(dayNumber);

        List<Period> periods = await _periodRepository.GetByWeekAndDay(week, day, cancellationToken);
        List<PeriodId> periodIds = periods.Select(period => period.Id).ToList();

        foreach (Offering offering in offerings)
        {
            List<Session> sessions = offering.Sessions
                .Where(session => 
                    periodIds.Contains(session.PeriodId) &&
                    !session.IsDeleted)
                .ToList();

            if (sessions.Count == 0)
                continue;

            Resource teamResource = offering.Resources
                .FirstOrDefault(resource => resource.Type == ResourceType.MicrosoftTeam);

            foreach (Session session in sessions)
            {
                Period period = periods.FirstOrDefault(period => period.Id == session.PeriodId);
                
                response.Add(new OfferingTimetableResponse(
                    $"{(period.Timetable.Prefix == '\0' ? string.Empty : period.Timetable.Prefix)}{period.PeriodCode}",
                    TimeOnly.FromTimeSpan(period.StartTime),
                    TimeOnly.FromTimeSpan(period.EndTime),
                    offering.Id,
                    offering.Name,
                    teamResource?.Name ?? string.Empty,
                    teamResource?.Url ?? string.Empty,
                    false));
            }
        }

        foreach (Tutorial tutorial in tutorials)
        {
            List<TutorialSession> sessions = tutorial.Sessions
                .Where(session =>
                    periodIds.Contains(session.PeriodId) &&
                    !session.IsDeleted)
                .ToList();

            if (sessions.Count == 0)
                continue;

            TeamsResource resource = tutorial.Teams.FirstOrDefault();

            foreach (TutorialSession session in sessions)
            {
                Period period = periods.FirstOrDefault(period => period.Id == session.PeriodId);

                response.Add(new TutorialTimetableResponse(
                    $"{(period.Timetable.Prefix == '\0' ? string.Empty : period.Timetable.Prefix)}{period.PeriodCode}",
                    TimeOnly.FromTimeSpan(period.StartTime),
                    TimeOnly.FromTimeSpan(period.EndTime),
                    tutorial.Id,
                    tutorial.Name,
                    resource?.Name ?? string.Empty,
                    resource?.Url ?? string.Empty,
                    false));
            }
        }

        List<Cover> covers = await _coverRepository.GetCurrentForStaff(request.StaffId, cancellationToken);

        foreach (Cover cover in covers)
        {
            if (cover is AccessCover)
                continue;

            Offering offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

            if (offering is null)
                continue;

            List<Session> sessions = offering.Sessions
                .Where(session =>
                    periodIds.Contains(session.PeriodId) &&
                    !session.IsDeleted)
                .ToList();

            if (sessions.Count == 0)
                continue;

            Resource teamResource = offering.Resources
                .FirstOrDefault(resource => resource.Type == ResourceType.MicrosoftTeam);

            foreach (Session session in sessions)
            {
                Period period = periods.FirstOrDefault(period => period.Id == session.PeriodId);

                response.Add(new OfferingTimetableResponse(
                    $"{(period.Timetable.Prefix == '\0' ? string.Empty : period.Timetable.Prefix)}{period.PeriodCode}",
                    TimeOnly.FromTimeSpan(period.StartTime),
                    TimeOnly.FromTimeSpan(period.EndTime),
                    offering.Id,
                    offering.Name,
                    teamResource.Name,
                    teamResource.Url,
                    true));
            }
        }

        return response;
    }
}

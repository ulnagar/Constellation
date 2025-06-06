namespace Constellation.Core.Models.Timetables.Repositories;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Timetables.ValueObjects;
using Enums;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPeriodRepository
{
    Task<double> TotalDurationForCollectionOfPeriods(List<PeriodId> periodIds, CancellationToken cancellationToken = default);
    Task<List<Period>> GetByDayAndOfferingId(int day, OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Period>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Period>> GetAllFromTimetable(List<Timetable> timetables, CancellationToken cancellationToken = default);
    Task<List<Period>> GetForOfferingOnDay(OfferingId offeringId, DateTime absenceDate, PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);
    Task<List<Period>> GetForOfferingOnDay(OfferingId offeringId, DateOnly absenceDate, PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);
    Task<Period> GetById(PeriodId id, CancellationToken cancellationToken = default);
    Task<List<Period>> GetListFromIds(List<PeriodId> periodIds, CancellationToken cancellationToken = default);
    Task<List<Period>> GetCurrent(CancellationToken cancellationToken = default);
    Task<List<Period>> GetByWeekAndDay(PeriodWeek week, PeriodDay day, CancellationToken cancellationToken = default);

    void Insert(Period period);
}
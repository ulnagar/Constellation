namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITimetablePeriodRepository
{
    Task<double> TotalDurationForCollectionOfPeriods(List<int> PeriodIds, CancellationToken cancellationToken = default);
    Task<List<TimetablePeriod>> GetByDayAndOfferingId(int dayNumber, OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<TimetablePeriod>> GetAll(CancellationToken cancellationToken = default);
    Task<List<TimetablePeriod>> GetAllFromTimetable(List<string> timetables, CancellationToken cancellationToken = default);
    Task<List<TimetablePeriod>> GetForOfferingOnDay(OfferingId offeringId, DateTime absenceDate, int DayNumber, CancellationToken cancellationToken = default);
    Task<List<TimetablePeriod>> GetForOfferingOnDay(OfferingId offeringId, DateOnly absenceDate, int DayNumber, CancellationToken cancellationToken = default);
    Task<TimetablePeriod> GetById(int id, CancellationToken cancellationToken = default);

    ICollection<TimetablePeriod> AllFromDay(int day);
    Task<ICollection<TimetablePeriod>> ForSelectionAsync();
    Task<ICollection<TimetablePeriod>> ForGraphicalDisplayAsync();
    Task<TimetablePeriod> ForEditAsync(int id);

    Task<List<TimetablePeriod>> GetCurrent(CancellationToken cancellationToken = default);
}

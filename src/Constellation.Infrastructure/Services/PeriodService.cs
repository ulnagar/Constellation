namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models;

public class PeriodService : IPeriodService
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public PeriodService(
        IUnitOfWork unitOfWork, 
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<ServiceOperationResult<TimetablePeriod>> CreatePeriod(PeriodDto periodResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<TimetablePeriod>();

        // Validate entries
        if (periodResource.Id != null)
        {
            var checkPeriod = await _unitOfWork.Periods.ForEditAsync(periodResource.Id.Value);

            if (checkPeriod != null)
            {
                result.Success = false;
                result.Errors.Add($"A period with that Id already exists!");
                return result;
            }
        }

        var period = new TimetablePeriod
        {
            Day = periodResource.Day,
            Timetable = periodResource.Timetable,
            Period = periodResource.Period,
            StartTime = periodResource.StartTime,
            EndTime = periodResource.EndTime,
            Name = periodResource.Name,
            Type = periodResource.Type
        };

        _unitOfWork.Add(period);

        result.Success = true;
        result.Entity = period;

        return result;
    }

    public async Task<ServiceOperationResult<TimetablePeriod>> UpdatePeriod(int? id, PeriodDto periodResource)
    {
        // Set up return object
        var result = new ServiceOperationResult<TimetablePeriod>();

        // Validate entries
        if (periodResource.Id == null)
        {
            result.Success = false;
            result.Errors.Add($"A period with that Id cannot be found!");
            return result;
        }

        var period = await _unitOfWork.Periods.ForEditAsync(periodResource.Id.Value);

        if (periodResource.Day != 0)
            period.Day = periodResource.Day;

        if (periodResource.Period != 0)
            period.Period = periodResource.Period;

        period.Timetable = periodResource.Timetable;

        period.StartTime = periodResource.StartTime;
        period.EndTime = periodResource.EndTime;

        if (!string.IsNullOrWhiteSpace(periodResource.Name))
            period.Name = periodResource.Name;

        if (!string.IsNullOrWhiteSpace(periodResource.Type))
            period.Type = periodResource.Type;

        result.Success = true;
        result.Entity = period;

        return result;
    }

    public async Task RemovePeriod(int id)
    {
        // Validate entries
        var period = await _unitOfWork.Periods.ForEditAsync(id);

        if (period == null)
            return;

        period.IsDeleted = true;
        period.DateDeleted = _dateTime.Now;
    }
}

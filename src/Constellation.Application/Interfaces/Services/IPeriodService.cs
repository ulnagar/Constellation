namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

public interface IPeriodService
{
    Task<ServiceOperationResult<TimetablePeriod>> CreatePeriod(PeriodDto periodResource);
    Task<ServiceOperationResult<TimetablePeriod>> UpdatePeriod(int? id, PeriodDto periodResource);
    Task RemovePeriod(int id);
}

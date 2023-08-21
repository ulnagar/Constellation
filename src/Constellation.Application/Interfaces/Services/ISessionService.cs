using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ISessionService
    {
        Task<ServiceOperationResult<OfferingSession>> CreateSession(SessionDto sessionResource);
        Task<ServiceOperationResult<OfferingSession>> UpdateSession(int id, SessionDto sessionResource);
        Task RemoveSession(int id);

        Task<ServiceOperationResult<TimetablePeriod>> CreatePeriod(PeriodDto periodResource);
        Task<ServiceOperationResult<TimetablePeriod>> UpdatePeriod(int? id, PeriodDto periodResource);
        Task RemovePeriod(int id);
    }
}

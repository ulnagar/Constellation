using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IStaffService
    {
        Task<ServiceOperationResult<Staff>> CreateStaffMember(StaffDto staffResource);
        Task<ServiceOperationResult<Staff>> UpdateStaffMember(string staffId, StaffDto staffResource);
        Task RemoveStaffMember(string staffId);
        Task ReinstateStaffMember(string staffId);
    }
}

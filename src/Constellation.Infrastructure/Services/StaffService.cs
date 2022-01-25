using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class StaffService : IStaffService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOperationService _operationService;
        private readonly ISessionService _sessionService;

        public StaffService(IUnitOfWork unitOfWork, ISessionService sessionService,
            IOperationService operationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _operationService = operationService;
        }

        public async Task<ServiceOperationResult<Staff>> CreateStaffMember(StaffDto staffResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Staff>();

            if (await _unitOfWork.Staff.AnyWithId(staffResource.StaffId))
            {
                result.Success = false;
                result.Errors.Add($"A staff member with that ID already exists!");

                return result;
            }

            var staff = new Staff()
            {
                StaffId = staffResource.StaffId,
                FirstName = staffResource.FirstName,
                LastName = staffResource.LastName,
                PortalUsername = staffResource.PortalUsername,
                AdobeConnectPrincipalId = staffResource.AdobeConnectPrincipalId,
                SchoolCode = staffResource.SchoolCode,
                Faculty = staffResource.Faculty
            };

            _unitOfWork.Add(staff);

            result.Success = true;
            result.Entity = staff;

            return result;
        }

        public async Task ReinstateStaffMember(string staffId)
        {
            // Validate entries
            var staff = await _unitOfWork.Staff.ForEditAsync(staffId);

            if (staff == null)
                return;

            staff.IsDeleted = false;
            staff.DateDeleted = null;
        }

        public async Task RemoveStaffMember(string staffId)
        {
            // Validate entries
            var staff = await _unitOfWork.Staff.ForDeletion(staffId);

            if (staff == null)
                return;

            // Remove any open sessions
            foreach (var session in staff.CourseSessions.Where(s => !s.IsDeleted).ToList())
            {
                await _sessionService.RemoveSession(session.Id);
            }

            // Process operations
            foreach (var operation in staff.AdobeConnectOperations.Where(a => !a.IsCompleted && !a.IsDeleted))
            {
                _operationService.CancelAdobeConnectOperation(operation);
            }

            staff.IsDeleted = true;
            staff.DateDeleted = DateTime.Now;
        }

        public async Task<ServiceOperationResult<Staff>> UpdateStaffMember(string staffId, StaffDto staffResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Staff>();

            // Validate entries
            var staff = await _unitOfWork.Staff.ForEditAsync(staffId);

            if (staff == null)
            {
                result.Success = false;
                result.Errors.Add($"A staff member with that ID could not be found!");

                return result;
            }

            // Update Properties
            if (!string.IsNullOrWhiteSpace(staffResource.StaffId))
                staff.StaffId = staffResource.StaffId;

            if (!string.IsNullOrWhiteSpace(staffResource.FirstName))
                staff.FirstName = staffResource.FirstName;

            if (!string.IsNullOrWhiteSpace(staffResource.LastName))
                staff.LastName = staffResource.LastName;

            if (!string.IsNullOrWhiteSpace(staffResource.PortalUsername))
                staff.PortalUsername = staffResource.PortalUsername;

            if (!string.IsNullOrWhiteSpace(staffResource.AdobeConnectPrincipalId))
                staff.AdobeConnectPrincipalId = staffResource.AdobeConnectPrincipalId;

            if (!string.IsNullOrWhiteSpace(staffResource.SchoolCode))
                staff.SchoolCode = staffResource.SchoolCode;

            if (staffResource.Faculty != 0)
                staff.Faculty = staffResource.Faculty;

            result.Success = true;
            result.Entity = staff;

            return result;
        }
    }
}

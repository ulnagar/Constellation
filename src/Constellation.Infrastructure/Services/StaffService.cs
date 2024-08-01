using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Offerings.RemoveTeacherFromAllOfferings;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;

namespace Constellation.Infrastructure.Services
{
    using Core.Models.Faculties;
    using Core.Models.Faculties.Repositories;

    // Reviewed for ASYNC Operations
    public class StaffService : IStaffService, IScopedService
    {
        private readonly ISender _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOperationService _operationService;
        private readonly IFacultyRepository _facultyRepository;

        public StaffService(
            ISender mediator,
            IUnitOfWork unitOfWork,
            IFacultyRepository facultyRepository,
            IOperationService operationService)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _facultyRepository = facultyRepository;
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
                IsShared = staffResource.IsShared
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

            // Remove teacher from offerings
            await _mediator.Send(new RemoveTeacherFromAllOfferingsCommand(staffId));

            staff.IsDeleted = true;
            staff.DateDeleted = DateTime.Now;

            foreach (FacultyMembership membership in staff.Faculties)
            {
                if (membership.IsDeleted) 
                    continue;

                Faculty faculty = await _facultyRepository.GetById(membership.FacultyId);
                faculty.RemoveMember(staff.StaffId);
            }
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

            staff.IsShared = staffResource.IsShared;

            result.Success = true;
            result.Entity = staff;

            return result;
        }
    }
}

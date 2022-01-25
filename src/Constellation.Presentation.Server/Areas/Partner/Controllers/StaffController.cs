using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class StaffController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IStaffService _staffService;
        private readonly IOperationService _operationsService;

        public StaffController(IUnitOfWork unitOfWork, IAuthService authService,
            IStaffService staffService, IOperationService operationsService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _staffService = staffService;
            _operationsService = operationsService;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => true);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutACDetails()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => string.IsNullOrWhiteSpace(staff.AdobeConnectPrincipalId) && !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromFaculty(string id)
        {
            _ = Enum.TryParse(id, out Faculty faculty);

            var staff = await _unitOfWork.Staff.ForListAsync(staff => staff.Faculty.HasFlag(faculty) && !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();

            return View("Index", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var staff = await _unitOfWork.Staff.ForEditAsync(id);
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<Staff_UpdateViewModel>();
            viewModel.Staff = new StaffDto
            {
                StaffId = staff.StaffId,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                PortalUsername = staff.PortalUsername,
                Faculty = staff.Faculty,
                SchoolCode = staff.SchoolCode,
                AdobeConnectPrincipalId = staff.AdobeConnectPrincipalId
            };
            viewModel.IsNew = false;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name", staff.SchoolCode);

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Staff_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                var schools = await _unitOfWork.Schools.ForSelectionAsync();
                viewModel.SchoolList = new SelectList(schools, "Code", "Name", viewModel.Staff.SchoolCode);
                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                var result = await _staffService.CreateStaffMember(viewModel.Staff);

                if (result.Success)
                {
                    await _unitOfWork.CompleteAsync();

                    await _operationsService.AddStaffToAdobeGroupBasedOnFaculty(result.Entity.StaffId, result.Entity.Faculty);

                    await _operationsService.CreateTeacherEmployedMSTeamAccess(result.Entity.StaffId);

                    await _operationsService.CreateCanvasUserFromStaff(result.Entity);

                    var user = new UserTemplateDto
                    {
                        FirstName = result.Entity.FirstName,
                        LastName = result.Entity.LastName,
                        Email = result.Entity.EmailAddress,
                        Username = result.Entity.EmailAddress,
                        IsStaffMember = true,
                        StaffId = result.Entity.StaffId
                    };

                    await _authService.CreateUser(user);
                }
            }
            else
            {
                var staff = await _unitOfWork.Staff.ForEditAsync(viewModel.Staff.StaffId);
                var originalFaculty = staff.Faculty;

                var result = await _staffService.UpdateStaffMember(viewModel.Staff.StaffId, viewModel.Staff);

                if (staff.Faculty != result.Entity.Faculty)
                {
                    // Create operations for group addition/removal.
                    await _operationsService.AuditStaffAdobeGroupMembershipBasedOnFaculty(staff.StaffId, originalFaculty, staff.Faculty);
                }

                var newUser = new UserTemplateDto
                {
                    FirstName = result.Entity.FirstName,
                    LastName = result.Entity.LastName,
                    Email = result.Entity.EmailAddress,
                    Username = result.Entity.EmailAddress,
                    IsStaffMember = true,
                    StaffId = result.Entity.StaffId
                };

                await _authService.UpdateUser(staff.EmailAddress, newUser);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<Staff_UpdateViewModel>();
            viewModel.IsNew = true;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var staff = await _unitOfWork.Staff.ForDetailDisplayAsync(id);

            if (staff == null)
            {
                return RedirectToAction("Index");
            }

            // Build the master form view model
            var viewModel = await CreateViewModel<Staff_DetailsViewModel>();
            viewModel.Staff = Staff_DetailsViewModel.StaffDto.ConvertFromStaff(staff);
            viewModel.Offerings = staff.CourseSessions.Where(session => !session.IsDeleted && session.Offering.EndDate >= DateTime.Now).Select(session => session.Offering).Distinct().Select(Staff_DetailsViewModel.OfferingDto.ConvertFromOffering).ToList();
            viewModel.Sessions = staff.CourseSessions.Where(session => !session.IsDeleted && session.Offering.EndDate >= DateTime.Now).Select(Staff_DetailsViewModel.SessionDto.ConvertFromSession).ToList();
            viewModel.SchoolStaff = staff.School.StaffAssignments.Where(role => !role.IsDeleted).Select(Staff_DetailsViewModel.ContactDto.ConvertFromRoleAssignment).ToList();

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Resign(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var staffMember = _unitOfWork.Staff.WithDetails(id);

            if (staffMember == null)
            {
                return RedirectToAction("Index");
            }

            if (staffMember.IsDeleted)
            {
                return RedirectToAction("Index");
            }

            await _staffService.RemoveStaffMember(staffMember.StaffId);

            await _operationsService.RemoveTeacherEmployedMSTeamAccess(staffMember.StaffId);

            await _operationsService.DisableCanvasUser(staffMember.StaffId);

            // Remove user access
            var newUser = new UserTemplateDto
            {
                FirstName = staffMember.FirstName,
                LastName = staffMember.LastName,
                Email = staffMember.EmailAddress,
                Username = staffMember.EmailAddress,
                IsStaffMember = false
            };

            await _authService.UpdateUser(staffMember.EmailAddress, newUser);

            await _operationsService.RemoveStaffAdobeGroupMembershipBasedOnFaculty(staffMember.StaffId, staffMember.Faculty);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Reinstate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var staffMember = await _unitOfWork.Staff.FromIdForExistCheck(id);

            if (staffMember == null)
            {
                return RedirectToAction("Index");
            }

            if (!staffMember.IsDeleted)
            {
                return RedirectToAction("Index");
            }

            await _staffService.ReinstateStaffMember(staffMember.StaffId);

            await _operationsService.CreateTeacherEmployedMSTeamAccess(staffMember.StaffId);

            // Reinstate user access
            var newUser = new UserTemplateDto
            {
                FirstName = staffMember.FirstName,
                LastName = staffMember.LastName,
                Email = staffMember.EmailAddress,
                Username = staffMember.EmailAddress,
                IsStaffMember = false
            };

            await _authService.UpdateUser(staffMember.EmailAddress, newUser);

            await _operationsService.AddStaffToAdobeGroupBasedOnFaculty(staffMember.StaffId, staffMember.Faculty);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id });
        }
    }
}
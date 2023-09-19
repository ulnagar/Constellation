using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Features.StaffMembers.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.Offerings.GetSessionListForTeacher;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class StaffController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IStaffService _staffService;
        private readonly IOperationService _operationsService;
        private readonly IDateTimeProvider _dateTime;
        private readonly IMediator _mediator;

        public StaffController(IUnitOfWork unitOfWork,
            IAuthService authService,
            IStaffService staffService, 
            IOperationService operationsService, 
            IDateTimeProvider dateTime,
            IMediator mediator)
            : base(mediator)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _staffService = staffService;
            _operationsService = operationsService;
            _dateTime = dateTime;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => true);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutACDetails()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => string.IsNullOrWhiteSpace(staff.AdobeConnectPrincipalId) && !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromFaculty(Guid facultyId)
        {
            var staff = await _unitOfWork.Staff.ForListAsync(staff => staff.Faculties.Any(member => member.FacultyId == facultyId && !member.IsDeleted) && !staff.IsDeleted);

            var viewModel = await CreateViewModel<Staff_ViewModel>();
            viewModel.Staff = staff.Select(Staff_ViewModel.StaffDto.ConvertFromStaff).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

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
                SchoolCode = staff.SchoolCode,
                AdobeConnectPrincipalId = staff.AdobeConnectPrincipalId,
                IsShared = staff.IsShared,
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

                var result = await _staffService.UpdateStaffMember(viewModel.Staff.StaffId, viewModel.Staff);

                var newUser = new UserTemplateDto
                {
                    FirstName = result.Entity.FirstName,
                    LastName = result.Entity.LastName,
                    Email = result.Entity.EmailAddress,
                    Username = result.Entity.EmailAddress,
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

            List<Staff_DetailsViewModel.OfferingDto> offeringResponse = new();

            var offerings = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(id));

            foreach (var entry in offerings.Value)
            {
                offeringResponse.Add(new()
                {
                    Id = entry.OfferingId,
                    Name = entry.OfferingName,
                    CourseName = entry.CourseName
                });
            }

            List<Staff_DetailsViewModel.SessionDto> sessionResponse = new();

            var sessions = await _mediator.Send(new GetSessionListForTeacherQuery(id));

            foreach (var entry in sessions.Value)
            {
                sessionResponse.Add(new()
                {
                    Period = entry.PeriodName,
                    ClassName = entry.OfferingName,
                    Duration = (int)entry.Duration
                });
            }

            // Build the master form view model
            var viewModel = await CreateViewModel<Staff_DetailsViewModel>();
            viewModel.Staff = Staff_DetailsViewModel.StaffDto.ConvertFromStaff(staff);
            viewModel.Offerings = offeringResponse;
            viewModel.Sessions = sessionResponse;
            viewModel.SchoolStaff = staff.School.StaffAssignments.Where(role => !role.IsDeleted).Select(Staff_DetailsViewModel.ContactDto.ConvertFromRoleAssignment).ToList();
            viewModel.Faculties = staff.Faculties.Where(membership => !membership.IsDeleted).Select(Staff_DetailsViewModel.FacultyDto.ConvertFromFacultyMembership).ToList();

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

            //await _operationsService.RemoveStaffAdobeGroupMembershipBasedOnFaculty(staffMember.StaffId, staffMember.Faculty);

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

            //await _operationsService.AddStaffToAdobeGroupBasedOnFaculty(staffMember.StaffId, staffMember.Faculty);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> DeleteFacultyRole(Guid membershipId)
        {
            await _mediator.Send(new RemoveFacultyMembershipFromStaffMemberCommand { MembershipId = membershipId });

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> _AddFacultyMembership(TeacherAddFacultySelection viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", new { id = viewModel.StaffId });

            await _mediator.Send(new CreateFacultyMembershipForStaffMemberCommand { StaffId = viewModel.StaffId, FacultyId = viewModel.FacultyId, Role = viewModel.Role });

            return RedirectToAction("Details", new { id = viewModel.StaffId });
        }
    }
}
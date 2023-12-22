namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Application.StaffMembers.AddStaffToFaculty;
using Application.StaffMembers.RemoveStaffFromFaculty;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.Offerings.GetSessionListForTeacher;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;
using Core.Models.Faculty;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.Repositories;
using Core.Models.Faculty.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("Partner")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class StaffController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IStaffService _staffService;
    private readonly IOperationService _operationsService;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IMediator _mediator;

    public StaffController(IUnitOfWork unitOfWork,
        IAuthService authService,
        IStaffService staffService, 
        IOperationService operationsService,
        IFacultyRepository facultyRepository,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _staffService = staffService;
        _operationsService = operationsService;
        _facultyRepository = facultyRepository;
        _mediator = mediator;
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        var staff = await _unitOfWork.Staff.ForEditAsync(id);
        var schools = await _unitOfWork.Schools.ForSelectionAsync();

        var viewModel = new Staff_UpdateViewModel();
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

        return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Create()
    {
        var schools = await _unitOfWork.Schools.ForSelectionAsync();

        var viewModel = new Staff_UpdateViewModel();
        viewModel.IsNew = true;
        viewModel.SchoolList = new SelectList(schools, "Code", "Name");

        return View("Update", viewModel);
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        var staff = await _unitOfWork.Staff.ForDetailDisplayAsync(id);

        if (staff == null)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        List<Staff_DetailsViewModel.OfferingDto> offeringResponse = new();

        var offerings = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(id));

        foreach (var entry in offerings.Value)
        {
            offeringResponse.Add(new()
            {
                Id = entry.OfferingId,
                Name = entry.OfferingName,
                CourseName = entry.CourseName,
                AssignmentType = entry.AssignmentType
            });
        }

        List<Staff_DetailsViewModel.SessionDto> sessionResponse = new();

        var sessions = await _mediator.Send(new GetSessionListForTeacherQuery(id));

        foreach (var entry in sessions.Value)
        {
            sessionResponse.Add(new()
            {
                PeriodSortOrder = entry.PeriodSortOrder,
                Period = entry.PeriodName,
                ClassName = entry.OfferingName,
                Duration = (int)entry.Duration
            });
        }

        // Build the master form view model
        Staff_DetailsViewModel viewModel = new Staff_DetailsViewModel();
        viewModel.Staff = Staff_DetailsViewModel.StaffDto.ConvertFromStaff(staff);
        viewModel.Offerings = offeringResponse;
        viewModel.Sessions = sessionResponse;
        viewModel.SchoolStaff = staff.School.StaffAssignments.Where(role => !role.IsDeleted).Select(Staff_DetailsViewModel.ContactDto.ConvertFromRoleAssignment).ToList();

        foreach (FacultyMembership membership in staff.Faculties)
        {
            if (membership.IsDeleted) continue;

            Faculty faculty = await _facultyRepository.GetById(membership.FacultyId);

            Staff_DetailsViewModel.FacultyDto dto = new(
                membership.Id,
                faculty.Id,
                faculty.Name,
                membership.Role);

            viewModel.Faculties.Add(dto);
        }

        return View(viewModel);
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Resign(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        var staffMember = _unitOfWork.Staff.WithDetails(id);

        if (staffMember == null)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        if (staffMember.IsDeleted)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
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
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        var staffMember = await _unitOfWork.Staff.FromIdForExistCheck(id);

        if (staffMember == null)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
        }

        if (!staffMember.IsDeleted)
        {
            return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
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
    public async Task<IActionResult> DeleteFacultyRole(string staffId, Guid faculty)
    {
        FacultyId facultyId = FacultyId.FromValue(faculty);

        await _mediator.Send(new RemoveStaffFromFacultyCommand(staffId, facultyId));

        return RedirectToPage("/Staff/Index", routeValues: new { area = "Partner" });
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> _AddFacultyMembership(TeacherAddFacultySelection viewModel)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Details", new { id = viewModel.StaffId });

        FacultyMembershipRole role = FacultyMembershipRole.FromValue(viewModel.Role);
        FacultyId facultyId = FacultyId.FromValue(viewModel.FacultyId);

        await _mediator.Send(new AddStaffToFacultyCommand(viewModel.StaffId, facultyId, role));

        return RedirectToAction("Details", new { id = viewModel.StaffId });
    }
}
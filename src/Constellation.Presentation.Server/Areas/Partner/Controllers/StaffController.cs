namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Application.StaffMembers.AddStaffToFaculty;
using Application.StaffMembers.RemoveStaffFromFaculty;
using Application.Training.Models;
using Application.Training.Roles.GetTrainingRoleListForStaffMember;
using Application.Training.Roles.RemoveStaffMemberFromTrainingRole;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.Offerings.GetSessionListForTeacher;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using Core.Models.Faculty;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.Repositories;
using Core.Models.Faculty.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Shared.Pages.Shared.Components.TeacherAddFaculty;

[Area("Partner")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class StaffController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IStaffService _staffService;
    private readonly IOperationService _operationsService;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IMediator _mediator;

    public StaffController(IUnitOfWork unitOfWork,
        IAuthService authService,
        IStaffService staffService, 
        IOperationService operationsService,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _staffService = staffService;
        _operationsService = operationsService;
        _facultyRepository = facultyRepository;
        _contactRepository = contactRepository;
        _mediator = mediator;
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

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(staff.SchoolCode);

        // Build the master form view model
        Staff_DetailsViewModel viewModel = new Staff_DetailsViewModel();
        viewModel.Staff = Staff_DetailsViewModel.StaffDto.ConvertFromStaff(staff);
        viewModel.Offerings = offeringResponse;
        viewModel.Sessions = sessionResponse;

        foreach (SchoolContact contact in contacts)
        {
            foreach (SchoolContactRole assignment in contact.Assignments.Where(role => !role.IsDeleted && role.SchoolCode == staff.SchoolCode).ToList())
            {
                viewModel.SchoolStaff.Add(Staff_DetailsViewModel.ContactDto.ConvertFromRoleAssignment(contact, assignment));
            }
        }

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

        //TODO: Move all this to Domain Events

        await _staffService.RemoveStaffMember(staffMember.StaffId);

        await _operationsService.RemoveTeacherEmployedMSTeamAccess(staffMember.StaffId);

        await _operationsService.DisableCanvasUser(staffMember.StaffId);

        Result<List<TrainingRoleResponse>> roleRequest = await _mediator.Send(new GetTrainingRoleListForStaffMemberQuery(id));

        if (roleRequest.IsSuccess)
        {
            foreach (TrainingRoleResponse entry in roleRequest.Value)
                await _mediator.Send(new RemoveStaffMemberFromTrainingRoleCommand(entry.Id, id));
        }

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
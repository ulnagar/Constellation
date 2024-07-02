namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Features.API.Schools.Queries;
using Application.Models.Auth;
using Application.Schools.GetSchoolsFromList;
using Application.StaffMembers.GetStaffById;
using Application.StaffMembers.GetStaffForSelectionList;
using Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Stocktake.RegisterSighting;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class SubmitModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;

    public SubmitModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_Dashboard;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty]
    public string SerialNumber { get; set; }
    [BindProperty]
    public string AssetNumber { get; set; }
    [BindProperty]
    public string Description { get; set; }
    [BindProperty]
    public string LocationCategory { get; set; }
    [BindProperty]
    public string LocationName { get; set; }
    [BindProperty]
    public string LocationCode { get; set; }
    [BindProperty]
    public string UserType { get; set; }
    [BindProperty]
    public string UserName { get; set; }
    [BindProperty]
    public string UserCode { get; set; }
    [BindProperty]
    public string? Comment { get; set; }

    public List<StudentDto> StudentList { get; set; } = new();
    public List<StaffSelectionListResponse> StaffList { get; set; } = new();
    public List<SchoolDto> SchoolList { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        RegisterSightingCommand command = new(
            Id,
            SerialNumber,
            AssetNumber,
            Description,
            LocationCategory,
            LocationName,
            LocationCode,
            UserType,
            UserName,
            UserCode,
            Comment,
            _currentUserService.EmailAddress,
            DateTime.Now);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            await PreparePage();

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Equipment/Stocktake/Dashboard", new { area = "Staff", Id });
    }

    private async Task PreparePage()
    {
        string staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(staffId))
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(staffId));

        if (staffMember.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        Result<List<StudentDto>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(staffMember.Value.SchoolCode));

        if (students.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        StudentList = students.Value
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (teachers.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                teachers.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        StaffList = teachers.Value
            .OrderBy(teacher => teacher.LastName)
            .ToList();

        ICollection<string> schoolCodes = await _mediator.Send(new GetSchoolCodeOfAllPartnerSchoolsQuery());

        Result<List<SchoolDto>> schools = await _mediator.Send(new GetSchoolsFromListQuery(schoolCodes.ToList()));

        if (schools.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        SchoolList = schools.Value
            .OrderBy(school => school.Name)
            .ToList();
    }
}
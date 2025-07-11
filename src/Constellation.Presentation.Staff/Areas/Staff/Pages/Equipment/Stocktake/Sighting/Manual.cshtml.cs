namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake.Sighting;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterManualSighting;
using Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolCodes;
using Constellation.Application.Domains.Schools.Queries.GetSchoolsFromList;
using Constellation.Application.Domains.StaffMembers.Models;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;
using Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Application.Domains.Students.Models;
using Constellation.Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Stocktake.Enums;
using Constellation.Core.Models.Stocktake.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ManualModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public ManualModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<ManualModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_Dashboard;
    [ViewData] public string PageTitle => "Stocktake Sighting Submission";

    [BindProperty(SupportsGet = true)]
    public StocktakeEventId Id { get; set; }
    [BindProperty]
    public string SerialNumber { get; set; }
    [BindProperty]
    public string Description { get; set; }
    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public LocationCategory LocationCategory { get; set; }
    [BindProperty]
    public string LocationName { get; set; }
    [BindProperty]
    public string LocationCode { get; set; }
    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public UserType UserType { get; set; }
    [BindProperty]
    public string UserName { get; set; }
    [BindProperty]
    public string UserCode { get; set; }
    [BindProperty]
    public string? Comment { get; set; }

    public List<StudentResponse> StudentList { get; set; } = new();
    public List<StaffSelectionListResponse> StaffList { get; set; } = new();

    public StaffResponse CurrentStaffMember { get; set; }
    public List<SchoolDto> SchoolList { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        RegisterManualSightingCommand command = new(
            Id,
            SerialNumber,
            Description,
            LocationCategory,
            LocationName,
            LocationCode,
            UserType,
            UserName,
            UserCode,
            Comment);

        _logger
            .ForContext(nameof(RegisterManualSightingCommand), command, true)
            .Information("Requested to add stocktake sighting by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            await PreparePage();

            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add stocktake sighting by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage("/Equipment/Stocktake/Dashboard", new { area = "Staff", Id });
    }

    private async Task PreparePage()
    {
        string claimStaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(claimStaffId))
        {
            ModalContent = ErrorDisplay.Create(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));
            
            return;
        }

        Guid guidStaffId = Guid.Parse(claimStaffId);
        StaffId staffId = StaffId.FromValue(guidStaffId);

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(staffId));

        if (staffMember.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        CurrentStaffMember = staffMember.Value;

        Result<List<StudentResponse>> students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(staffMember.Value.SchoolCode));

        if (students.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                students.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        StudentList = students.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();

        Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (teachers.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                teachers.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        StaffList = teachers.Value
            .OrderBy(teacher => teacher.Name.SortOrder)
            .ToList();

        Result<List<string>> schoolCodes = await _mediator.Send(new GetCurrentPartnerSchoolCodesQuery());

        if (schoolCodes.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schoolCodes.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        Result<List<SchoolDto>> schools = await _mediator.Send(new GetSchoolsFromListQuery(schoolCodes.Value));

        if (schools.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schools.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Dashboard", values: new { area = "Staff" }));

            return;
        }

        SchoolList = schools.Value
            .OrderBy(school => school.Name)
            .ToList();
    }
}
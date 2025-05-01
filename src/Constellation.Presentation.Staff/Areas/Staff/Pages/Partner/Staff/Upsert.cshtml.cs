namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Common.PresentationModels;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Application.Domains.StaffMembers.Commands.CreateStaffMember;
using Application.Domains.StaffMembers.Commands.UpdateStaffMember;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSchools)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;
    [ViewData] public string PageTitle { get; set; } = "New Staff Member";

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; } = string.Empty;

    [BindProperty]
    public string StaffId { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string PortalUsername { get; set; } = string.Empty;

    [BindProperty]
    public string SchoolCode { get; set; } = string.Empty;

    [BindProperty]
    public bool IsShared { get; set; }

    public SelectList SchoolList { get; set; }

    public async Task OnGet()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            return;
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            SchoolList = new(schools.Value, "Code", "Name");

            return;
        }

        _logger
            .Information("Requested to retrieve Staff Member with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffMember.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), staffMember.Error, true)
                .Information("Failed to retrieve Staff Member with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StaffId = staffMember.Value.StaffId;
        FirstName = staffMember.Value.Name.FirstName;
        LastName = staffMember.Value.Name.LastName;
        PortalUsername = staffMember.Value.PortalUsername;
        SchoolCode = staffMember.Value.SchoolCode;
        IsShared = staffMember.Value.IsShared;

        SchoolList = new(schools.Value, "Code", "Name", staffMember.Value.SchoolCode);

        PageTitle = $"Editing {staffMember.Value.Name.DisplayName}";
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            // Create new student
            CreateStaffMemberCommand createCommand = new(
                StaffId,
                FirstName,
                LastName,
                PortalUsername,
                SchoolCode,
                IsShared);

            _logger
                .ForContext(nameof(CreateStaffMemberCommand), createCommand, true)
                .Information("Requested to create new Staff Member by user {User}", _currentUserService.UserName);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                ModalContent = new ErrorDisplay(createResult.Error);

                _logger
                    .ForContext(nameof(Error), createResult.Error, true)
                    .Warning("Failed to create new Staff Member by user {User}", _currentUserService.UserName);

                Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

                SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

                return Page();
            }

            return RedirectToPage("/Partner/Staff/Index", new { area = "Staff" });
        }

        // Edit existing student
        UpdateStaffMemberCommand updateCommand = new(
            StaffId,
            FirstName,
            LastName,
            PortalUsername,
            SchoolCode,
            IsShared);

        _logger
            .ForContext(nameof(UpdateStaffMemberCommand), updateCommand, true)
            .Information("Requested to update Staff Member with id {Id} by user {User}", StaffId, _currentUserService.UserName);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(updateResult.Error);

            _logger
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Staff Member with id {Id} by user {User}", StaffId, _currentUserService.UserName);

            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        return RedirectToPage("/Partner/Staff/Index", new { area = "Staff" });
    }
}
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Common.PresentationModels;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Application.Domains.StaffMembers.Commands.CreateStaffMember;
using Application.Domains.StaffMembers.Commands.UpdateStaffMember;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using Core.Models;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
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
    public StaffId Id { get; set; } = StaffId.Empty;

    [BindProperty]
    public string? EmployeeId { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string PreferredName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public Gender Gender { get; set; }

    [BindProperty]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    public string SchoolCode { get; set; } = string.Empty;

    [BindProperty]
    public bool IsShared { get; set; }

    public SelectList SchoolList { get; set; }
    public SelectList GenderList { get; set; }

    public async Task OnGet()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            return;
        }
        
        IEnumerable<Gender> genders = Gender.GetOptions;

        GenderList = new(genders, "Value", "Value");

        if (Id == StaffId.Empty)
        {
            SchoolList = new(schools.Value, "Code", "Name");

            return;
        }

        _logger
            .Information("Requested to retrieve Staff Member with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffMember.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                staffMember.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), staffMember.Error, true)
                .Information("Failed to retrieve Staff Member with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            return;
        }

        EmployeeId = staffMember.Value.EmployeeId?.Number ?? string.Empty;
        FirstName = staffMember.Value.Name.FirstName;
        PreferredName = staffMember.Value.Name.PreferredName;
        LastName = staffMember.Value.Name.LastName;
        Gender = staffMember.Value.Gender;
        EmailAddress = staffMember.Value.EmailAddress.Email;
        SchoolCode = staffMember.Value.SchoolCode;
        IsShared = staffMember.Value.IsShared;

        SchoolList = new(schools.Value, nameof(School.Code), nameof(School.Name), staffMember.Value.SchoolCode);

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

        if (Id == StaffId.Empty)
        {
            // Create new student
            CreateStaffMemberCommand createCommand = new(
                EmployeeId,
                FirstName,
                PreferredName,
                LastName,
                Gender,
                EmailAddress,
                SchoolCode,
                IsShared);

            _logger
                .ForContext(nameof(CreateStaffMemberCommand), createCommand, true)
                .Information("Requested to create new Staff Member by user {User}", _currentUserService.UserName);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(createResult.Error);

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
            Id,
            EmployeeId,
            FirstName,
            PreferredName,
            LastName,
            Gender,
            EmailAddress,
            SchoolCode,
            IsShared);

        _logger
            .ForContext(nameof(UpdateStaffMemberCommand), updateCommand, true)
            .Information("Requested to update Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(updateResult.Error);

            _logger
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        return RedirectToPage("/Partner/Staff/Index", new { area = "Staff" });
    }
}
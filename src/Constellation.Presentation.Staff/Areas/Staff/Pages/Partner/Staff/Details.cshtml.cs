namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Commands.AddStaffToFaculty;
using Application.Domains.StaffMembers.Commands.ReinstateStaffMember;
using Application.Domains.StaffMembers.Commands.RemoveStaffFromFaculty;
using Application.Domains.StaffMembers.Commands.ResignStaffMember;
using Application.Domains.StaffMembers.Queries.GetLifecycleDetailsForStaffMember;
using Application.Domains.StaffMembers.Queries.GetStaffDetails;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Queries.GetLifecycleDetailsForStudent;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.TeacherAddFaculty;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;
    [ViewData] public string PageTitle { get; set; } = "Staff Details";

    [BindProperty(SupportsGet = true)]
    public StaffId Id { get; set; } = StaffId.Empty;

    public StaffDetailsResponse StaffMember { get; set; }

    public RecordLifecycleDetailsResponse RecordLifecycle { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));

        if (staffRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), staffRequest.Error, true)
                .Warning("Failed to retrieve details of Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        StaffMember = staffRequest.Value;

        PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

        Result<RecordLifecycleDetailsResponse> recordLifecycle = await _mediator.Send(new GetLifecycleDetailsForStaffMemberQuery(Id));

        RecordLifecycle = recordLifecycle.IsSuccess
            ? recordLifecycle.Value
            : new RecordLifecycleDetailsResponse(
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                null);
    }

    public async Task<IActionResult> OnGetResign()
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        ResignStaffMemberCommand command = new(Id);

        _logger
            .ForContext(nameof(ResignStaffMemberCommand), command, true)
            .Information("Requested to resign Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to resign Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReinstate()
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        ReinstateStaffMemberCommand command = new(Id);

        _logger
            .ForContext(nameof(ReinstateStaffMemberCommand), command, true)
            .Information("Requested to reinstate Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);
            
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate Staff Member with id {Id} by user {User}", Id, _currentUserService.UserName);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddFacultyRole(TeacherAddFacultySelection viewModel)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }
        
        FacultyMembershipRole role = FacultyMembershipRole.FromValue(viewModel.Role);
        FacultyId facultyId = FacultyId.FromValue(viewModel.FacultyId);

        AddStaffToFacultyCommand command = new(viewModel.StaffId, facultyId, role);
        
        _logger
            .ForContext(nameof(AddStaffToFacultyCommand), command, true)
            .Information("Requested to add Staff Member with id {Id} to Faculty by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add Staff Member with id {Id} to Faculty by user {User}", Id, _currentUserService.UserName);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDeleteFacultyRole(Guid facultyId)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        FacultyId facultyIdent = FacultyId.FromValue(facultyId);

        RemoveStaffFromFacultyCommand command = new(Id, facultyIdent);

        _logger
            .ForContext(nameof(RemoveStaffFromFacultyCommand), command, true)
            .Information("Requested to remove Staff Member with id {Id} from Faculty by user {User}", Id, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove Staff Member with id {Id} from Faculty by user {User}", Id, _currentUserService.UserName);
            
            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            PageTitle = $"Details - {StaffMember.StaffName.DisplayName}";

            return Page();
        }

        return RedirectToPage();
    }
}
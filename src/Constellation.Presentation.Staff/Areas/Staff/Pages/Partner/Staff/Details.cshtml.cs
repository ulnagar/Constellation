namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Models.Auth;
using Application.StaffMembers.GetLifecycleDetailsForStaffMember;
using Application.StaffMembers.GetStaffDetails;
using Application.StaffMembers.ReinstateStaffMember;
using Application.StaffMembers.ResignStaffMember;
using Constellation.Application.StaffMembers.AddStaffToFaculty;
using Constellation.Application.StaffMembers.RemoveStaffFromFaculty;
using Constellation.Application.Students.GetLifecycleDetailsForStudent;
using Core.Errors;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Pages.Shared.Components.TeacherAddFaculty;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    public StaffDetailsResponse StaffMember { get; set; }

    public RecordLifecycleDetailsResponse RecordLifecycle { get; set; }

    public async Task OnGet()
    {
        Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));

        if (staffRequest.IsFailure)
        {
            Error = new()
            {
                Error = staffRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" })
            };

            return;
        }

        StaffMember = staffRequest.Value;

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
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        Result result = await _mediator.Send(new ResignStaffMemberCommand(Id));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReinstate()
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        Result result = await _mediator.Send(new ReinstateStaffMemberCommand(Id));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddFacultyRole(TeacherAddFacultySelection viewModel)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;
            
            return Page();
        }
        
        FacultyMembershipRole role = FacultyMembershipRole.FromValue(viewModel.Role);
        FacultyId facultyId = FacultyId.FromValue(viewModel.FacultyId);

        Result result = await _mediator.Send(new AddStaffToFacultyCommand(viewModel.StaffId, facultyId, role));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDeleteFacultyRole(Guid facultyId)
    {
        AuthorizationResult authorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanEditStaff);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        FacultyId facultyIdent = FacultyId.FromValue(facultyId);

        Result result = await _mediator.Send(new RemoveStaffFromFacultyCommand(Id, facultyIdent));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<StaffDetailsResponse> staffRequest = await _mediator.Send(new GetStaffDetailsQuery(Id));
            StaffMember = staffRequest.Value;

            return Page();
        }

        return RedirectToPage();
    }
}
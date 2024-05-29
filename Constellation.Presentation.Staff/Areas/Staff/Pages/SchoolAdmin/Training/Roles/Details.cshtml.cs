namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Roles;

using Application.Models.Auth;
using Application.Training.Roles.AddModuleToTrainingRole;
using Application.Training.Roles.AddStaffMemberToTrainingRole;
using Application.Training.Roles.GetTrainingRoleDetails;
using Application.Training.Roles.RemoveModuleFromTrainingRole;
using Application.Training.Roles.RemoveStaffMemberFromTrainingRole;
using Core.Errors;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Pages.Shared.Components.AddModuleToTrainingRole;
using Presentation.Shared.Pages.Shared.Components.AddStaffMemberToTrainingRole;
using Presentation.Shared.Pages.Shared.PartialViews.RemoveModuleFromTrainingRoleModal;
using Presentation.Shared.Pages.Shared.PartialViews.RemoveStaffMemberFromTrainingRoleModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        IAuthorizationService authService,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _authService = authService;
        _linkGenerator = linkGenerator;
    }
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TrainingRoleDetailResponse Role { get; set; }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Roles;

    public async Task OnGet()
    {
        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result<TrainingRoleDetailResponse> request = await _mediator.Send(new GetTrainingRoleDetailsQuery(roleId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Index", values: new { area = "Staff" })
            };

            return;
        }

        Role = request.Value;
    }

    public IActionResult OnPostAjaxRemoveMember(
        string roleId,
        string staffId,
        string staffName,
        string roleName)
    {
        Guid roleGuid = Guid.Parse(roleId);

        TrainingRoleId RoleId = TrainingRoleId.FromValue(roleGuid);

        RemoveStaffMemberFromTrainingRoleModalViewModel viewModel = new(
            RoleId,
            staffId,
            staffName,
            roleName);

        return Partial("RemoveStaffMemberFromTrainingRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveStaff(string staffId)
    {
        AuthorizationResult canEdit = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEdit.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new RemoveStaffMemberFromTrainingRoleCommand(roleId, staffId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", Id });
    }

    public async Task<IActionResult> OnPostAddStaffMember(AddStaffMemberToTrainingRoleSelection viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = Core.Shared.Error.NullValue,
                RedirectPath = null
            };

            return Page();
        }

        AuthorizationResult canEdit = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEdit.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new AddStaffMemberToTrainingRoleCommand(roleId, new() { viewModel.StaffId }));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddModule(AddModuleToTrainingRoleSelection viewModel)
    {
        if (viewModel.ModuleId == Guid.Empty)
        {
            Error = new()
            {
                Error = Core.Shared.Error.NullValue,
                RedirectPath = null
            };

            return Page();
        }

        AuthorizationResult canEdit = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEdit.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);
        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);
        
        Result request = await _mediator.Send(new AddModuleToTrainingRoleCommand(roleId, new() { moduleId }));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxRemoveModule(
        string roleId,
        string roleName,
        string moduleId,
        string moduleName)
    {
        TrainingRoleId RoleId = TrainingRoleId.FromValue(Guid.Parse(roleId));
        TrainingModuleId ModuleId = TrainingModuleId.FromValue(Guid.Parse(moduleId));

        RemoveModuleFromTrainingRoleModalViewModel viewModel = new(
            RoleId,
            ModuleId,
            roleName,
            moduleName);

        return Partial("RemoveModuleFromTrainingRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveModule(Guid moduleId)
    {
        AuthorizationResult canEdit = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEdit.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);
        TrainingModuleId ModuleId = TrainingModuleId.FromValue(moduleId);

        Result request = await _mediator.Send(new RemoveModuleFromTrainingRoleCommand(roleId, ModuleId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", Id });
    }
}


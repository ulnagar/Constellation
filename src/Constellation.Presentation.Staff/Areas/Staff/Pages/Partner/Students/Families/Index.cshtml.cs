namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Families.GetFamilyById;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.DeleteFamilyById;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Core.Errors;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilyMemberConfirmationModal;
using Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilySelectionModal;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Family List";

    public List<FamilyContactResponse> Contacts { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPostAjaxDeleteFamily(
        [ModelBinder(typeof(StrongIdBinder))] FamilyId familyId,
        [ModelBinder(typeof(StrongIdBinder))] ParentId parentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(familyId));

        ParentResponse? parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == parentId);

        DeleteFamilySelectionModalViewModel viewModel = new()
        {
            FamilyName = family.Value?.FamilyName ?? string.Empty,
            ParentId = parentId,
            FamilyId = familyId,
            ParentName = parent?.ParentName ?? string.Empty,
            OtherParentNames = family.Value?.Parents
                .Except(new List<ParentResponse>{ parent })
                .Select(entry => entry.ParentName)
                .ToList()
        };

        return Partial("DeleteFamilySelectionModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteFamily(
        [ModelBinder(typeof(StrongIdBinder))] FamilyId id, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }

        Result result = await _mediator.Send(new DeleteFamilyByIdCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAjaxDeleteParent(
        [ModelBinder(typeof(StrongIdBinder))] FamilyId familyId,
        [ModelBinder(typeof(StrongIdBinder))] ParentId parentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(familyId));

        ParentResponse? parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == parentId);

        DeleteFamilyMemberConfirmationModalViewModel viewModel = new()
        {
            FamilyName = family.Value?.FamilyName ?? string.Empty,
            Title = "Remove parent from family",
            UserName = parent?.ParentName ?? string.Empty,
            FamilyId = familyId,
            ParentId = parentId
        };

        return Partial("DeleteFamilyMemberConfirmationModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteParent(
        [ModelBinder(typeof(StrongIdBinder))] FamilyId family, 
        [ModelBinder(typeof(StrongIdBinder))] ParentId parent, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }
        
        Result result = await _mediator.Send(new DeleteParentByIdCommand(family, parent), cancellationToken);
        
        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<List<FamilyContactResponse>> contactRequest = await _mediator.Send(new GetFamilyContactsQuery(), cancellationToken);

        if (contactRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = contactRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }

        Contacts = contactRequest.Value;

        return Page();
    }
}

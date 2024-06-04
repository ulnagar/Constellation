namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Families.GetFamilyById;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Families.DeleteFamilyById;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Presentation.Shared.Pages.Shared.PartialViews.ConfirmActionUpdateModal;
using Constellation.Presentation.Staff.Areas;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;

    public List<FamilyContactResponse> Contacts { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken)
    {
        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAjaxDeleteFamily(Guid familyId, Guid parentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(FamilyId.FromValue(familyId)));

        ParentResponse? parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == ParentId.FromValue(parentId));

        DeleteFamilySelectionModalViewModel viewModel = new()
        {
            FamilyName = family.Value?.FamilyName ?? string.Empty,
            ParentId = ParentId.FromValue(parentId),
            FamilyId = FamilyId.FromValue(familyId),
            ParentName = parent?.ParentName ?? string.Empty,
            OtherParentNames = family.Value?.Parents
                .Except(new List<ParentResponse>{ parent })
                .Select(entry => entry.ParentName)
                .ToList()
        };

        return Partial("DeleteFamilySelectionModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteFamily(Guid id, CancellationToken cancellationToken)
    {
        var authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }

        var familyId = FamilyId.FromValue(id);

        var result = await _mediator.Send(new DeleteFamilyByIdCommand(familyId), cancellationToken);

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

    public async Task<IActionResult> OnPostAjaxDeleteParent(Guid familyId, Guid parentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(FamilyId.FromValue(familyId)));

        ParentResponse? parent = family.Value.Parents.FirstOrDefault(parent => parent.ParentId == ParentId.FromValue(parentId));

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

    public async Task<IActionResult> OnGetDeleteParent(Guid family, Guid parent, CancellationToken cancellationToken)
    {
        var authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return Page();
        }
        
        var familyId = FamilyId.FromValue(family);
        var parentId = ParentId.FromValue(parent);

        var result = await _mediator.Send(new DeleteParentByIdCommand(familyId, parentId), cancellationToken);
        
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
        var contactRequest = await _mediator.Send(new GetFamilyContactsQuery(), cancellationToken);

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

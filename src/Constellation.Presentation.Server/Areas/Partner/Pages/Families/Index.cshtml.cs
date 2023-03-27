namespace Constellation.Presentation.Server.Areas.Partner.Pages.Families;

using Constellation.Application.Families.DeleteFamilyById;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public List<FamilyContactResponse> Contacts { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnGetDeleteFamily(Guid id, CancellationToken cancellationToken)
    {
        var authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnGetDeleteParent(Guid family, Guid parent, CancellationToken cancellationToken)
    {
        var authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return Page();
        }

        Contacts = contactRequest.Value;

        return Page();
    }
}

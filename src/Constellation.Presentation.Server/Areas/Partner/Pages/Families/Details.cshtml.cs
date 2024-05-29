namespace Constellation.Presentation.Server.Areas.Partner.Pages.Families;

using Constellation.Application.Families.AddStudentToFamily;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyDetailsById;
using Constellation.Application.Families.RemoveStudentFromFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Students.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Pages.Shared.Components.FamilyAddStudent;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authSevice;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authSevice)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authSevice = authSevice;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public FamilyDetailsResponse Family { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnGetRemoveStudent(string studentId, CancellationToken cancellationToken)
    {
        var authorised = await _authSevice.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        if (string.IsNullOrWhiteSpace(studentId))
        {
            Error = new()
            {
                Error = StudentErrors.InvalidId,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        var family = FamilyId.FromValue(Id);

        var result = await _mediator.Send(new RemoveStudentFromFamilyCommand(family, studentId), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnGetRemoveParent(Guid parentId, CancellationToken cancellationToken)
    {
        var authorised = await _authSevice.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        var family = FamilyId.FromValue(Id);
        var parent = ParentId.FromValue(parentId);

        var result = await _mediator.Send(new DeleteParentByIdCommand(family, parent), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAddStudent(FamilyAddStudentSelection viewModel, CancellationToken cancellationToken)
    {
        var authorised = await _authSevice.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
        {
            Error = new()
            {
                Error = StudentErrors.InvalidId,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        var family = FamilyId.FromValue(Id);

        var result = await _mediator.Send(new AddStudentToFamilyCommand(family, viewModel.StudentId), cancellationToken);

        if (result.IsSuccess)
        {
            return await PreparePage(cancellationToken);
        }

        Error = new()
        {
            Error = result.Error,
            RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
        };

        return Page();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        var familyId = FamilyId.FromValue(Id);

        var familyRequest = await _mediator.Send(new GetFamilyDetailsByIdQuery(familyId), cancellationToken);

        if (familyRequest.IsFailure)
        {
            Error = new()
            {
                Error = familyRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Families/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        Family = familyRequest.Value;

        return Page();
    }
}

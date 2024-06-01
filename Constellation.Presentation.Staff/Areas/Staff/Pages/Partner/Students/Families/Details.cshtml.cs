namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Students.GetStudentById;
using Application.Students.Models;
using Constellation.Application.Families.AddStudentToFamily;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.GetFamilyDetailsById;
using Constellation.Application.Families.Models;
using Constellation.Application.Families.RemoveStudentFromFamily;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.FamilyAddStudent;
using Constellation.Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilyMemberConfirmationModal;
using Constellation.Presentation.Shared.Pages.Shared.PartialViews.DeleteFamilySelectionModal;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public FamilyDetailsResponse Family { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
        => await PreparePage(cancellationToken);
    
    public async Task<IActionResult> OnPostAjaxDeleteStudent(Guid familyId, string studentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(FamilyId.FromValue(familyId)));

        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(studentId));

        DeleteFamilyMemberConfirmationModalViewModel viewModel = new()
        {
            FamilyName = family.Value?.FamilyName ?? string.Empty,
            Title = "Remove student from family",
            UserName = student.Value?.DisplayName ?? string.Empty,
            FamilyId = familyId,
            StudentId = studentId
        };

        return Partial("DeleteFamilyMemberConfirmationModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveStudent(string studentId, CancellationToken cancellationToken)
    {
        var authorised = await _authSevice.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        if (string.IsNullOrWhiteSpace(studentId))
        {
            Error = new()
            {
                Error = StudentErrors.InvalidId,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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

    public async Task<IActionResult> OnGetRemoveParent(Guid parentId, CancellationToken cancellationToken)
    {
        var authorised = await _authSevice.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
        {
            Error = new()
            {
                Error = StudentErrors.InvalidId,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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
            RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        Family = familyRequest.Value;

        return Page();
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Students.GetStudentById;
using Application.Students.Models;
using Areas;
using Constellation.Application.Families.AddStudentToFamily;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyById;
using Constellation.Application.Families.GetFamilyDetailsById;
using Constellation.Application.Families.Models;
using Constellation.Application.Families.RemoveStudentFromFamily;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using Shared.Components.FamilyAddStudent;
using Shared.PartialViews.DeleteFamilyMemberConfirmationModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => Family is not null ? $"Family Details - {Family.FamilyTitle}" : "Family Details";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public FamilyId Id { get; set; }

    public FamilyDetailsResponse? Family { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
        => await PreparePage(cancellationToken);
    
    public async Task<IActionResult> OnPostAjaxDeleteStudent(
        [ModelBinder(typeof(StrongIdBinder))] FamilyId familyId, 
        string studentId)
    {
        Result<FamilyResponse> family = await _mediator.Send(new GetFamilyByIdQuery(familyId));

        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(studentId));

        DeleteFamilyMemberConfirmationModalViewModel viewModel = new()
        {
            FamilyName = family.Value?.FamilyName ?? string.Empty,
            Title = "Remove student from family",
            UserName = student.Value?.Name.DisplayName ?? string.Empty,
            FamilyId = familyId,
            StudentId = studentId
        };

        return Partial("DeleteFamilyMemberConfirmationModal", viewModel);
    }

    public async Task<IActionResult> OnGetRemoveStudent(string studentId, CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        if (string.IsNullOrWhiteSpace(studentId))
        {
            ModalContent = new ErrorDisplay(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        Result result = await _mediator.Send(new RemoveStudentFromFamilyCommand(Id, studentId), cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

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

    public async Task<IActionResult> OnGetRemoveParent(
        [ModelBinder(typeof(StrongIdBinder))] ParentId parentId, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        Result result = await _mediator.Send(new DeleteParentByIdCommand(Id, parentId), cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAddStudent(FamilyAddStudentSelection viewModel, CancellationToken cancellationToken)
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        if (string.IsNullOrWhiteSpace(viewModel.StudentId))
        {
            ModalContent = new ErrorDisplay(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        Result result = await _mediator.Send(new AddStudentToFamilyCommand(Id, viewModel.StudentId), cancellationToken);

        if (result.IsSuccess)
        {
            return await PreparePage(cancellationToken);
        }

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

        return Page();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        Result<FamilyDetailsResponse> familyRequest = await _mediator.Send(new GetFamilyDetailsByIdQuery(Id), cancellationToken);

        if (familyRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                familyRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        Family = familyRequest.Value;

        return Page();
    }
}

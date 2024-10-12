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
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.Components.FamilyAddStudent;
using Shared.PartialViews.DeleteFamilyMemberConfirmationModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle { get; set; } = "Family Details";
    
    [BindProperty(SupportsGet = true)]
    public FamilyId Id { get; set; }

    public FamilyDetailsResponse? Family { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
        => await PreparePage(cancellationToken);
    
    public async Task<IActionResult> OnPostAjaxDeleteStudent(
        FamilyId familyId,
        StudentId studentId)
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

    public async Task<IActionResult> OnGetRemoveStudent(
        StudentId studentId,
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

        if (studentId == StudentId.Empty)
        {
            ModalContent = new ErrorDisplay(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        RemoveStudentFromFamilyCommand command = new(Id, studentId);

        _logger
            .ForContext(nameof(RemoveStudentFromFamilyCommand), command, true)
            .Information("Requested to remove Student from Family by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove Student from Family by user {User}", _currentUserService.UserName);

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAjaxDeleteParent(
        FamilyId familyId,
        ParentId parentId)
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
        ParentId parentId, 
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

        DeleteParentByIdCommand command = new(Id, parentId);

        _logger
            .ForContext(nameof(DeleteParentByIdCommand), command, true)
            .Information("Requested to remove Parent from Family by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove Parent from Family by user {User}", _currentUserService.UserName);

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

        if (viewModel.StudentId == StudentId.Empty)
        {
            ModalContent = new ErrorDisplay(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            return Page();
        }

        AddStudentToFamilyCommand command = new(Id, viewModel.StudentId);

        _logger
            .ForContext(nameof(AddStudentToFamilyCommand), command, true)
            .Information("Requested to add Student to Family by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return await PreparePage(cancellationToken);
        }

        ModalContent = new ErrorDisplay(
            result.Error,
            _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

        _logger
            .ForContext(nameof(Error), result.Error, true)
            .Warning("Failed to add Student to Family by user {User}", _currentUserService.UserName);

        return Page();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        _logger
            .Information("Requested to retrieve details of Family with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<FamilyDetailsResponse> familyRequest = await _mediator.Send(new GetFamilyDetailsByIdQuery(Id), cancellationToken);

        if (familyRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                familyRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Families/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), familyRequest.Error, true)
                .Warning("Failed to retrieve details of Family with id {Id} by user {User}", Id, _currentUserService.UserName);

            return Page();
        }

        Family = familyRequest.Value;

        PageTitle = $"Details - {Family.FamilyTitle}";

        return Page();
    }
}

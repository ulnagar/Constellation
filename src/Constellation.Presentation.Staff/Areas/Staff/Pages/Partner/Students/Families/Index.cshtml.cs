namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Families;

using Application.Common.PresentationModels;
using Application.Families.GetFamilyById;
using Application.Models.Auth;
using Areas;
using Constellation.Application.Families.DeleteFamilyById;
using Constellation.Application.Families.DeleteParentById;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.PartialViews.DeleteFamilyMemberConfirmationModal;
using Shared.PartialViews.DeleteFamilySelectionModal;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
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
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Families;
    [ViewData] public string PageTitle => "Family List";

    public List<FamilyContactResponse> Contacts { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPostAjaxDeleteFamily(
        [ModelBinder(typeof(ConstructorBinder))] FamilyId familyId,
        [ModelBinder(typeof(ConstructorBinder))] ParentId parentId)
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
        [ModelBinder(typeof(ConstructorBinder))] FamilyId id, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        DeleteFamilyByIdCommand command = new(id);

        _logger
            .ForContext(nameof(DeleteFamilyByIdCommand), command, true)
            .Information("Requested to delete Family by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to delete Family by user {User}", _currentUserService.UserName);

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    public async Task<IActionResult> OnPostAjaxDeleteParent(
        [ModelBinder(typeof(ConstructorBinder))] FamilyId familyId,
        [ModelBinder(typeof(ConstructorBinder))] ParentId parentId)
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
        [ModelBinder(typeof(ConstructorBinder))] FamilyId family, 
        [ModelBinder(typeof(ConstructorBinder))] ParentId parent, 
        CancellationToken cancellationToken)
    {
        AuthorizationResult authorized = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditStudents);

        if (!authorized.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Permissions.Unauthorised,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        DeleteParentByIdCommand command = new(family, parent);

        _logger
            .ForContext(nameof(DeleteParentByIdCommand), command, true)
            .Information("Requested to delete Parent by user {User}", _currentUserService.UserName);
        
        Result result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to delete Parent by user {User}", _currentUserService.UserName);

            return Page();
        }

        return await PreparePage(cancellationToken);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve list of Families by user {User}", _currentUserService.UserName);

        Result<List<FamilyContactResponse>> contactRequest = await _mediator.Send(new GetFamilyContactsQuery(), cancellationToken);

        if (contactRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                contactRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), contactRequest.Error, true)
                .Warning("Failed to retrieve list of Families by user {User}", _currentUserService.UserName);

            return Page();
        }

        Contacts = contactRequest.Value;

        return Page();
    }
}

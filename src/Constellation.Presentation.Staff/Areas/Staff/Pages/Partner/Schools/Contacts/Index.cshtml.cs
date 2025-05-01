namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.RepairSchoolContactUser;
using Application.Domains.SchoolContacts.Commands.CreateContactRoleAssignment;
using Application.Domains.SchoolContacts.Commands.RemoveContactRole;
using Application.Domains.SchoolContacts.Commands.UpdateRoleNote;
using Application.Domains.SchoolContacts.Models;
using Application.Domains.SchoolContacts.Queries.GetAllContacts;
using Application.Domains.SchoolContacts.Queries.GetContactRolesForSelectionList;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Application.Models.Auth;
using Constellation.Core.Models.SchoolContacts.Enums;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.SchoolContacts.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.PartialViews.AssignRoleModal;
using Shared.PartialViews.DeleteRoleModal;
using Shared.PartialViews.UpdateRoleNoteModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Contacts;
    [ViewData] public string PageTitle => "School Contacts List";
    
    [BindProperty(SupportsGet = true)]
    public GetAllContactsQuery.SchoolContactFilter Filter { get; set; } = GetAllContactsQuery.SchoolContactFilter.WithRole;

    public List<SchoolContactResponse> Contacts { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of school contacts by user {User}", _currentUserService.UserName);

        Result<List<SchoolContactResponse>> contacts;

        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.IsExecutive);

        contacts = await _mediator.Send(new GetAllContactsQuery(Filter, execMemberTest.Succeeded));

        if (contacts.IsFailure)
        {
            ModalContent = new ErrorDisplay(contacts.Error);

            _logger
                .ForContext(nameof(Error), contacts.Error, true)
                .Warning("Failed to retrieve list of school contacts by user {User}", _currentUserService.UserName);

            return;
        }
        
        Contacts = Filter switch
        {
            GetAllContactsQuery.SchoolContactFilter.All => contacts.Value.OrderBy(entry => entry.SchoolName).ToList(),
            GetAllContactsQuery.SchoolContactFilter.WithRole => contacts.Value.Where(entry => entry.AssignmentId != SchoolContactRoleId.Empty).OrderBy(entry => entry.SchoolName).ToList(),
            GetAllContactsQuery.SchoolContactFilter.WithoutRole => contacts.Value.Where(entry => entry.AssignmentId != SchoolContactRoleId.Empty).OrderBy(entry => entry.Name).ToList(),
            _ => contacts.Value.OrderBy(entry => entry.SchoolName).ToList()
        };
    }

    public async Task<IActionResult> OnPostAjaxUpdateNote(
        SchoolContactId contactId,
        SchoolContactRoleId roleId,
        string note)
    {
        UpdateRoleNoteModalViewModel viewModel = new()
        {
            ContactId = contactId,
            RoleId = roleId,
            Note = note
        };

        return Partial("UpdateRoleNoteModal", viewModel);
    }

    public async Task<IActionResult> OnPostUpdateNote(
        SchoolContactId contactId,
        SchoolContactRoleId roleId,
        string note)
    {
        UpdateRoleNoteCommand command = new(contactId, roleId, note);

        _logger
            .ForContext(nameof(UpdateRoleNoteCommand), command, true)
            .Information("Requested to update note of School Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner" }));

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update note of School Contact by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxAssign(
        SchoolContactId contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        AuthorizationResult execMemberTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.IsExecutive);

        Result<List<Position>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery(execMemberTest.Succeeded));
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value);
        viewModel.ContactId = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnPostCreateAssignment(
        string schoolCode,
        Position roleName,
        string note,
        SchoolContactId contactId)
    {
        CreateContactRoleAssignmentCommand command = new(contactId, schoolCode, roleName, note);

        _logger
            .ForContext(nameof(CreateContactRoleAssignmentCommand), command, true)
            .Information("Requested to create assignment for School Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner" }));

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create assignment for School Contact by user {User}", _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxDelete(
        SchoolContactId contactId,
        SchoolContactRoleId roleId,
        string name,
        string role,
        string school)
    {
        DeleteRoleModalViewModel viewModel = new(
            contactId,
            roleId,
            name,
            role,
            school);

        return Partial("DeleteRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(
        SchoolContactId contactId,
        SchoolContactRoleId roleId)
    {
        RemoveContactRoleCommand command = new(contactId, roleId);

        _logger
            .ForContext(nameof(RemoveContactRoleCommand), command, true)
            .Information("Requested to remove assignment for School Contact by user {User}", _currentUserService.UserName);

        await _mediator.Send(command);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRepairUser(SchoolContactId id)
    {
        RepairSchoolContactUserCommand command = new(id);

        _logger
            .ForContext(nameof(RepairSchoolContactUserCommand), command, true)
            .Information("Requested to repair user account for School Contact by user {User}", _currentUserService.UserName);

        await _mediator.Send(command);

        return RedirectToPage();
    }
}
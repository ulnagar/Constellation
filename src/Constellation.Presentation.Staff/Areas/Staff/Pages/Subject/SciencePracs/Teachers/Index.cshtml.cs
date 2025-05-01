namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.RepairSchoolContactUser;
using Application.Domains.SchoolContacts.Commands.CreateContactRoleAssignment;
using Application.Domains.SchoolContacts.Commands.RemoveContactRole;
using Application.Domains.SchoolContacts.Commands.UpdateRoleNote;
using Application.Domains.SchoolContacts.Models;
using Application.Domains.SchoolContacts.Queries.GetAllSciencePracTeachers;
using Application.Domains.SchoolContacts.Queries.GetContactRolesForSelectionList;
using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.SchoolContacts.Enums;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;
    [ViewData] public string PageTitle => "Science Prac Teacher List";

    public List<SchoolContactResponse> Contacts = new();

    public async Task OnGet() => await PreparePage();

    public async Task OnGetAudit(SchoolContactId id)
    {
        _logger.Information("Requested to audit user details for School Contact with id {Id} by user {User}", id, _currentUserService.UserName);

        Result auditRequest = await _mediator.Send(new RepairSchoolContactUserCommand(id));

        if (auditRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), auditRequest.Error, true)
                .Warning("Failed to audit user details for School Contact with id {Id} by user {User}", id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(auditRequest.Error);

            await PreparePage();
        }
    }

    public IActionResult OnPostAjaxDelete(
        SchoolContactId contactId,
        SchoolContactRoleId roleId,
        string name,
        Position role,
        string school)
    {
        DeleteRoleModalViewModel viewModel = new(
            contactId,
            roleId,
            name,
            role.Name,
            school);

        return Partial("DeleteRoleModal", viewModel);
    }

    public async Task<IActionResult> OnPostAjaxAssign(
        SchoolContactId contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        Result<List<Position>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery(false));
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value, Position.SciencePracticalTeacher);
        viewModel.RoleName = Position.SciencePracticalTeacher;
        viewModel.ContactId = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(
        SchoolContactId contactId,
        SchoolContactRoleId roleId)
    {
        RemoveContactRoleCommand command = new(contactId, roleId);

        _logger
            .ForContext(nameof(RemoveContactRoleCommand), command, true)
            .Information("Requested to remove role from Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove role from Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
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
            .Information("Requested to assign role to Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Information("Requested to assign role to Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
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
            .Information("Requested to update Note for Contact by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update Note for Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve list of Science Prac Teachers by user {User}", _currentUserService.UserName);

        Result<List<SchoolContactResponse>> contactRequest = await _mediator.Send(new GetAllSciencePracTeachersQuery());

        if (contactRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), contactRequest.Error, true)
                .Warning("Failed to retrieve list of Science Prac Teachers by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(contactRequest.Error);

            return;
        }

        Contacts = contactRequest.Value;
    }
}

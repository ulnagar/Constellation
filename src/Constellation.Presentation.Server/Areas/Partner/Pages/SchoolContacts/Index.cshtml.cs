namespace Constellation.Presentation.Server.Areas.Partner.Pages.SchoolContacts;

using Application.Models.Auth;
using Application.SchoolContacts.CreateContactRoleAssignment;
using Application.SchoolContacts.GetAllContacts;
using Application.SchoolContacts.GetContactRolesForSelectionList;
using Application.SchoolContacts.UpdateRoleNote;
using Application.Schools.GetSchoolsForSelectionList;
using Application.Schools.Models;
using Application.Users.RepairSchoolContactUser;
using BaseModels;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Shared;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Shared.Helpers.ModelBinders;
using Server.Pages.Shared.PartialViews.AssignRoleModal;
using Server.Pages.Shared.PartialViews.DeleteRoleModal;
using Server.Pages.Shared.PartialViews.UpdateRoleNoteModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => SchoolContactsPages.Contacts;

    [BindProperty(SupportsGet = true)]
    public GetAllContactsQuery.SchoolContactFilter Filter { get; set; } = GetAllContactsQuery.SchoolContactFilter.WithRole;

    public List<SchoolContactResponse> Contacts { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<SchoolContactResponse>> contacts = await _mediator.Send(new GetAllContactsQuery(Filter));

        if (contacts.IsFailure)
        {
            Error = new()
            {
                Error = contacts.Error,
                RedirectPath = null
            };

            return;
        }

        Contacts = Filter switch
        {
            GetAllContactsQuery.SchoolContactFilter.All => contacts.Value.OrderBy(entry => entry.SchoolName).ToList(),
            GetAllContactsQuery.SchoolContactFilter.WithRole => contacts.Value.Where(entry => entry.AssignmentId is not null).OrderBy(entry => entry.SchoolName).ToList(),
            GetAllContactsQuery.SchoolContactFilter.WithoutRole => contacts.Value.Where(entry => entry.AssignmentId is null).OrderBy(entry => entry.Name).ToList(),
            _ => contacts.Value.OrderBy(entry => entry.SchoolName).ToList()
        };
    }

    public async Task<IActionResult> OnPostAjaxUpdateNote(
        Guid contactId,
        Guid roleId,
        string note)
    {
        UpdateRoleNoteModalViewModel viewModel = new()
        {
            ContactId = SchoolContactId.FromValue(contactId),
            RoleId = SchoolContactRoleId.FromValue(roleId),
            Note = note
        };

        return Partial("UpdateRoleNoteModal", viewModel);
    }

    public async Task<IActionResult> OnPostUpdateNote(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId,
        string note)
    {
        Result request = await _mediator.Send(new UpdateRoleNoteCommand(contactId, roleId, note));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxAssign(
        Guid contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value);
        viewModel.ContactGuid = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnPostCreateAssignment(
        string schoolCode,
        string roleName,
        string note,
        Guid contactGuid)
    {
        SchoolContactId contactId = SchoolContactId.FromValue(contactGuid);

        Result request = await _mediator.Send(new CreateContactRoleAssignmentCommand(contactId, schoolCode, roleName, note));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner" })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostAjaxDelete(
        Guid contactId,
        Guid roleId,
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

    public async Task<IActionResult> OnGetDeleteAssignment(Guid contactGuid, Guid roleGuid)
    {
        SchoolContactId contactId = SchoolContactId.FromValue(contactGuid);
        SchoolContactRoleId roleId = SchoolContactRoleId.FromValue(roleGuid);

        await _mediator.Send(new RemoveContactRoleCommand(contactId, roleId));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRepairUser(Guid id)
    {
        SchoolContactId contactId = SchoolContactId.FromValue(id);

        await _mediator.Send(new RepairSchoolContactUserCommand(contactId));

        return RedirectToPage();
    }
}
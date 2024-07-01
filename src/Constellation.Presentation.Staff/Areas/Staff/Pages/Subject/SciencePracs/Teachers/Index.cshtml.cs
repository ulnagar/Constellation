namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Constellation.Application.SchoolContacts.UpdateRoleNote;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Views.Shared.PartialViews.AssignRoleModal;
using Views.Shared.PartialViews.DeleteRoleModal;
using Views.Shared.PartialViews.UpdateRoleNoteModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;

    public List<ContactResponse> Contacts = new();

    public async Task OnGet()
    {
        Result<List<ContactResponse>> contactRequest = await _mediator.Send(new GetAllSciencePracTeachersQuery());

        if (contactRequest.IsFailure)
        {
            Error = new()
            {
                Error = contactRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Contacts = contactRequest.Value.OrderBy(contact => contact.ContactName.SortOrder).ToList();
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

    public async Task<IActionResult> OnPostAjaxAssign(
        Guid contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value, SchoolContactRole.SciencePrac);
        viewModel.RoleName = SchoolContactRole.SciencePrac;
        viewModel.ContactGuid = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(Guid contactGuid, Guid roleGuid)
    {
        SchoolContactId contactId = SchoolContactId.FromValue(contactGuid);
        SchoolContactRoleId roleId = SchoolContactRoleId.FromValue(roleGuid);

        await _mediator.Send(new RemoveContactRoleCommand(contactId, roleId));

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        return RedirectToPage("Index", new { area = "Subject" });
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
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/SchoolContacts/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        return RedirectToPage();
    }
}

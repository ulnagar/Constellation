namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Application.Common.PresentationModels;
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
using Shared.PartialViews.AssignRoleModal;
using Shared.PartialViews.DeleteRoleModal;
using Shared.PartialViews.UpdateRoleNoteModal;

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
            ModalContent = new ErrorDisplay(contactRequest.Error);

            return;
        }

        Contacts = contactRequest.Value.OrderBy(contact => contact.ContactName.SortOrder).ToList();
    }

    public IActionResult OnPostAjaxDelete(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId,
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
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        viewModel.Schools = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value, SchoolContactRole.SciencePrac);
        viewModel.RoleName = SchoolContactRole.SciencePrac;
        viewModel.ContactId = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId)
    {
        await _mediator.Send(new RemoveContactRoleCommand(contactId, roleId));

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostCreateAssignment(
        string schoolCode,
        string roleName,
        string note,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId)
    {
        Result request = await _mediator.Send(new CreateContactRoleAssignmentCommand(contactId, schoolCode, roleName, note));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostAjaxUpdateNote(
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId,
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
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactId contactId,
        [ModelBinder(typeof(StrongIdBinder))] SchoolContactRoleId roleId,
        string note)
    {
        Result request = await _mediator.Send(new UpdateRoleNoteCommand(contactId, roleId, note));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Partner/SchoolContacts/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage();
    }
}

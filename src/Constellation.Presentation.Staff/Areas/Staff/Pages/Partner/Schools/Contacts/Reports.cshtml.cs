namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.ExportContactsBySchool;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.SchoolContacts.GetContactsBySchool;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Application.Users.RepairSchoolContactUser;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Shared.PartialViews.AssignRoleModal;
using Shared.PartialViews.DeleteRoleModal;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ReportsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_ContactReports;

    public List<SchoolWithContactsResponse> Schools { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<SchoolWithContactsResponse>> schools = await _mediator.Send(new GetContactsBySchoolQuery());

        if (schools.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff"}));

            return;
        }

        Schools = schools.Value.OrderBy(entry => entry.SchoolName).ToList();
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<FileDto> file = await _mediator.Send(new ExportContactsBySchoolQuery());

        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(file.Error);

            Result<List<SchoolWithContactsResponse>> schools = await _mediator.Send(new GetContactsBySchoolQuery());
            Schools = schools.Value.OrderBy(entry => entry.SchoolName).ToList();
            
            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
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
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

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
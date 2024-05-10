namespace Constellation.Presentation.Server.Areas.Partner.Pages.SchoolContacts;

using Application.DTOs;
using Application.Models.Auth;
using Application.SchoolContacts.ExportContactsBySchool;
using Application.SchoolContacts.GetContactsBySchool;
using Application.Users.RepairSchoolContactUser;
using BaseModels;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.AssignRoleModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteRoleModal;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Constellation.Application.Models.EmailQueue.EmailQueueItem.EmailQueueReferenceType.LessonRolls;

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

    [ViewData] public string ActivePage => SchoolContactsPages.Reports;

    public List<SchoolWithContactsResponse> Schools { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<SchoolWithContactsResponse>> schools = await _mediator.Send(new GetContactsBySchoolQuery());

        if (schools.IsFailure)
        {
            Error = new()
            {
                Error = schools.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner"})
            };

            return;
        }

        Schools = schools.Value.OrderBy(entry => entry.SchoolName).ToList();
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<FileDto> file = await _mediator.Send(new ExportContactsBySchoolQuery());

        if (file.IsFailure)
        {
            Error = new()
            {
                Error = file.Error,
                RedirectPath = null
            };

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
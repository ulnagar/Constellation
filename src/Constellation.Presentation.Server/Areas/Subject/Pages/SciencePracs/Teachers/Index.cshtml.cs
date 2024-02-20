namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Teachers;

using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.SchoolContacts.RemoveContactRole;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.AssignRoleModal;
using Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteRoleModal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    public string ActivePage => SubjectPages.Teachers;

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
        int roleId,
        string name,
        string role,
        string school)
    {
        DeleteRoleModalViewModel viewModel = new(
            roleId,
            name,
            role,
            school);

        return Partial("DeleteRoleModal", viewModel);
    }

    public async Task<IActionResult> OnPostAjaxAssign(
        int contactId,
        string name)
    {
        AssignRoleModalViewModel viewModel = new();

        var rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        var schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        viewModel.Schools = new SelectList(schoolsRequest.Value, "Code", "Name");
        viewModel.Roles = new SelectList(rolesRequest.Value, SchoolContactRole.SciencePrac);
        viewModel.RoleName = SchoolContactRole.SciencePrac;
        viewModel.ContactId = contactId;
        viewModel.ContactName = name;

        return Partial("AssignRoleModal", viewModel);
    }

    public async Task<IActionResult> OnGetDeleteAssignment(int Id)
    {
        await _mediator.Send(new RemoveContactRoleCommand(Id));

        return RedirectToPage("Index", new { area = "Subject" });
    }

    public async Task<IActionResult> OnPostCreateAssignment(
        string SchoolCode,
        string RoleName,
        int ContactId)
    {
        Result request = await _mediator.Send(new CreateContactRoleAssignmentCommand(ContactId, SchoolCode, RoleName));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SciencePracs/Teachers/Index", values: new { area = "Subject" })
            };

            return Page();
        }

        return RedirectToPage("Index", new { area = "Subject" });
    }
}

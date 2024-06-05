namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Constellation.Application.AdminDashboards.VerifySchoolContactAccess;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Users.RepairSchoolContactUser;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class AuditModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AuditModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public UserAuditDto Audit { get; set; }

    public async Task OnGet()
    {
        SchoolContactId contactId = SchoolContactId.FromValue(Id);

        Result<UserAuditDto> auditRequest = await _mediator.Send(new VerifySchoolContactAccessQuery(contactId));

        if (auditRequest.IsFailure)
        {
            Error = new()
            {
                Error = auditRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" })
            };

            return;
        }

        Audit = auditRequest.Value;
    }

    public async Task<IActionResult> OnGetRepair()
    {
        SchoolContactId contactId = SchoolContactId.FromValue(Id);

        await _mediator.Send(new RepairSchoolContactUserCommand(contactId));

        return RedirectToPage("/Subject/SciencePracs/Teachers/Audit", new { area = "Staff" });
    }
}

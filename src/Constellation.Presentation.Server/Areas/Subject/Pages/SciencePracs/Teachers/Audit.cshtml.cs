namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Teachers;

using Application.Users.RepairSchoolContactUser;
using Constellation.Application.AdminDashboards.VerifySchoolContactAccess;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.SchoolContacts.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [ViewData] public string ActivePage => SubjectPages.Teachers;

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
                RedirectPath = _linkGenerator.GetPathByPage("/SciencePracs/Teachers/Index", values: new { area = "Subject" })
            };

            return;
        }

        Audit = auditRequest.Value;
    }

    public async Task<IActionResult> OnGetRepair()
    {
        SchoolContactId contactId = SchoolContactId.FromValue(Id);

        await _mediator.Send(new RepairSchoolContactUserCommand(contactId));

        return RedirectToPage("/SciencePracs/Teachers/Audit", new { area = "Subject" });
    }
}

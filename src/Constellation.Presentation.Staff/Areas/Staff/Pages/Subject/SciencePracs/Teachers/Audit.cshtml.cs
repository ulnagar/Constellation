namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Application.Common.PresentationModels;
using Application.Models.Identity;
using Constellation.Application.AdminDashboards.VerifySchoolContactAccess;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Users.RepairSchoolContactUser;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class AuditModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AuditModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AuditModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;
    [ViewData] public string PageTitle => "User Audit";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public SchoolContactId Id { get; set; }

    public UserAuditDto Audit { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to audit user details for School Contact with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<UserAuditDto> auditRequest = await _mediator.Send(new VerifySchoolContactAccessQuery(Id));

        if (auditRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), auditRequest.Error, true)
                .Warning("Failed to audit user details for School Contact with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                auditRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" }));

            return;
        }

        Audit = auditRequest.Value;
    }

    public async Task<IActionResult> OnGetRepair()
    {
        RepairSchoolContactUserCommand command = new(Id);

        _logger
            .ForContext(nameof(RepairSchoolContactUserCommand), command, true)
            .Information("Requested to repair user account for School Contact by user {User}", _currentUserService.UserName);

        Result<AppUser> result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), command, true)
                .Warning("Failed to repair user account for School Contact by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" }));
                
            return Page();
        }

        return RedirectToPage();
    }
}

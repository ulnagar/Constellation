namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Common.PresentationModels;
using Application.Domains.WorkFlows.Queries.GetCaseSummaryList;
using Application.Models.Auth;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.WorkFlow.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;
    [ViewData] public string PageTitle => "WorkFlow Cases";

    public List<CaseSummaryResponse> Cases { get; set; } = new();

    [BindProperty(SupportsGet=true)]
    public CaseStatusFilter Filter { get; set; } = CaseStatusFilter.Open;

    public bool IsAdmin { get; set; } = false;

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of WorkFlow Cases by user {User}", _currentUserService.UserName);

        AuthorizationResult isAdminTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (isAdminTest.Succeeded)
            IsAdmin = true;

        string claimStaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;
        Guid guidStaffId = Guid.Parse(claimStaffId);
        StaffId staffId = StaffId.FromValue(guidStaffId);

        Result<List<CaseSummaryResponse>> request = await _mediator.Send(new GetCaseSummaryListQuery(IsAdmin, staffId, Filter));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of WorkFlow Cases by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Cases = request.Value;
    }
}
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Common.PresentationModels;
using Application.Domains.WorkFlows.Queries.GetCaseSummaryList;
using Application.Models.Auth;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.WorkFlow.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;
    [ViewData] public string PageTitle => "WorkFlow Cases";

    public List<CaseSummaryResponse> Cases { get; set; } = new();

    [BindProperty(SupportsGet=true)]
    public FilterEnum Filter { get; set; } = FilterEnum.Open;

    public bool IsAdmin { get; set; } = false;

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of WorkFlow Cases by user {User}", _currentUserService.UserName);

        AuthorizationResult isAdminTest = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (isAdminTest.Succeeded)
            IsAdmin = true;

        string staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        Result<List<CaseSummaryResponse>> request = Filter switch
        {
            FilterEnum.ForMe => await _mediator.Send(new GetCaseSummaryListQuery(false, staffId)),
            _ => await _mediator.Send(new GetCaseSummaryListQuery(IsAdmin, staffId))
        };

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of WorkFlow Cases by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        Cases = Filter switch
        {
            FilterEnum.All => request.Value,
            FilterEnum.Open => request.Value.Where(entry => !entry.Status.Equals(CaseStatus.Completed) && !entry.Status.Equals(CaseStatus.Cancelled)).ToList(),
            FilterEnum.Closed => request.Value.Where(entry => entry.Status.Equals(CaseStatus.Completed) || entry.Status.Equals(CaseStatus.Cancelled)).ToList(),
            FilterEnum.Overdue => request.Value.Where(entry => (entry.Status.Equals(CaseStatus.Open) || entry.Status.Equals(CaseStatus.PendingAction)) && entry.DueDate < _dateTime.Today).ToList(),
            FilterEnum.ForMe => request.Value.Where(entry => !entry.Status.Equals(CaseStatus.Completed) && !entry.Status.Equals(CaseStatus.Cancelled)).ToList(),
            _ => request.Value
        };
    }

    public enum FilterEnum
    {
        All,
        Closed,
        Open,
        Overdue,
        ForMe
    }
}
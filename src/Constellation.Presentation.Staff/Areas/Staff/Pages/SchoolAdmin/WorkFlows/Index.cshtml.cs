namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Workflows;

using Application.Models.Auth;
using Application.WorkFlows.GetCaseSummaryList;
using Core.Abstractions.Clock;
using Core.Models.WorkFlow.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDateTimeProvider _dateTime;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _dateTime = dateTime;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Cases;

    public List<CaseSummaryResponse> Cases { get; set; }

    [BindProperty(SupportsGet=true)]
    public FilterEnum Filter { get; set; } = FilterEnum.Open;

    public async Task OnGet()
    {
        AuthorizationResult isAdmin = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        string staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        Result<List<CaseSummaryResponse>> request = await _mediator.Send(new GetCaseSummaryListQuery(isAdmin.Succeeded, staffId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Cases = Filter switch
        {
            FilterEnum.All => request.Value,
            FilterEnum.Open => request.Value.Where(entry => !entry.Status.Equals(CaseStatus.Completed) && !entry.Status.Equals(CaseStatus.Cancelled)).ToList(),
            FilterEnum.Closed => request.Value.Where(entry => entry.Status.Equals(CaseStatus.Completed) || entry.Status.Equals(CaseStatus.Cancelled)).ToList(),
            FilterEnum.Overdue => request.Value.Where(entry => (entry.Status.Equals(CaseStatus.Open) || entry.Status.Equals(CaseStatus.PendingAction)) && entry.DueDate < _dateTime.Today).ToList(),
            _ => request.Value
        };
    }

    public enum FilterEnum
    {
        All,
        Closed,
        Open,
        Overdue
    }
}
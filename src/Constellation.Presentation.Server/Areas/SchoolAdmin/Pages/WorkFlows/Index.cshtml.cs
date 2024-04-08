namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Workflows;

using Application.Models.Auth;
using Application.WorkFlows.GetCaseSummaryList;
using BaseModels;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
    }

    [ViewData] public string ActivePage => WorkFlowPages.Cases;

    public List<CaseSummaryResponse> Cases { get; set; }

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

        Cases = request.Value;
    }
}
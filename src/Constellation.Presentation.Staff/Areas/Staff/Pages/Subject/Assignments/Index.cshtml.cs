namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Application.Common.PresentationModels;
using Application.Domains.Assignments.Queries.GetCurrentAssignmentsListing;
using Constellation.Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;
    [ViewData] public string PageTitle => "Assignment List";

    [BindProperty(SupportsGet = true)]
    public GetCurrentAssignmentsListingQuery.Filter Filter { get; set; } = GetCurrentAssignmentsListingQuery.Filter.Current;

    public List<CurrentAssignmentSummaryResponse> Assignments { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve list of Assignments by user {User}", _currentUserService.UserName);

        Result<List<CurrentAssignmentSummaryResponse>> assignmentRequest = await _mediator.Send(new GetCurrentAssignmentsListingQuery(Filter), cancellationToken);

        if (assignmentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), assignmentRequest.Error, true)
                .Warning("Failed to retrieve list of Assignments by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(assignmentRequest.Error);

            return;
        }

        Assignments = assignmentRequest.Value;
    }
}

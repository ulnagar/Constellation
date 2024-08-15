namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Constellation.Application.Assignments.GetCurrentAssignmentsListing;
using Constellation.Application.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;

    public List<CurrentAssignmentSummaryResponse> Assignments { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        Result<List<CurrentAssignmentSummaryResponse>> assignmentRequest = await _mediator.Send(new GetCurrentAssignmentsListingQuery(), cancellationToken);

        if (assignmentRequest.IsFailure)
        {
            // not possible
        }

        Assignments = assignmentRequest.Value;

        return Page();
    }
}

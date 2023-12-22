namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments;

using Constellation.Application.Assignments.GetCurrentAssignmentsListing;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [ViewData] public string ActivePage => SubjectPages.Assignments;

    public List<CurrentAssignmentSummaryResponse> Assignments { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        var assignmentRequest = await _mediator.Send(new GetCurrentAssignmentsListingQuery(), cancellationToken);

        if (assignmentRequest.IsFailure)
        {
            // not possible
        }

        Assignments = assignmentRequest.Value;

        return Page();
    }
}

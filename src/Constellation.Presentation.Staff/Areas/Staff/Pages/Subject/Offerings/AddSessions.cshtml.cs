namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.AddMultipleSessionsToOffering;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.GetSessionListForOffering;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Periods.GetPeriodsForVisualSelection;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddSessionsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AddSessionsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    [BindProperty] 
    public List<int> Periods { get; set; } = new();

    public List<PeriodVisualSelectResponse> ValidPeriods { get; set; } = new();
    public List<SessionListResponse> ExistingSessions { get; set; } = new();

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        OfferingId offeringId = OfferingId.FromValue(Id);

        Result request = await _mediator.Send(new AddMultipleSessionsToOfferingCommand(offeringId, Periods));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        Result<List<PeriodVisualSelectResponse>> periodRequest =
            await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            return;
        }

        ValidPeriods = periodRequest.Value;

        // Get current periods linked to Offering

        OfferingId offeringId = OfferingId.FromValue(Id);

        Result<List<SessionListResponse>> sessionRequest =
            await _mediator.Send(new GetSessionListForOfferingQuery(offeringId));

        if (sessionRequest.IsFailure)
        {
            Error = new()
            {
                Error = sessionRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            return;
        }

        ExistingSessions = sessionRequest.Value;

        // Get CourseName and OfferingName

        Result<OfferingSummaryResponse> offeringRequest =
            await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

        if (offeringRequest.IsFailure)
        {
            Error = new()
            {
                Error = offeringRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id })
            };

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }

}
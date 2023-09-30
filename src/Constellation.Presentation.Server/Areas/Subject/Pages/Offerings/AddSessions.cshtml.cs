namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Application.Models.Auth;
using Application.Offerings.AddMultipleSessionsToOffering;
using Application.Offerings.GetOfferingSummary;
using Application.Offerings.GetSessionListForOffering;
using Application.Offerings.Models;
using Application.Periods.GetPeriodsForVisualSelection;
using BaseModels;
using Core.Models.Offerings.Identifiers;
using Core.Shared;
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

    [ViewData]
    public string ActivePage => "Offerings";

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
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Offerings/Details", new { area = "Subject", Id = Id });
    }

    private async Task PreparePage()
    {
        await GetClasses(_mediator);

        Result<List<PeriodVisualSelectResponse>> periodRequest =
            await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id })
            };

            return;
        }

        CourseName = offeringRequest.Value.CourseName;
        OfferingName = offeringRequest.Value.Name;
    }

}
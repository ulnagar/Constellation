namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Application.Models.Auth;
using Application.Periods.GetPeriodsForVisualSelection;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddSessionsModel : BasePageModel
{
    private readonly ISender _mediator;

    public AddSessionsModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string CourseName { get; set; }
    public string OfferingName { get; set; }

    [BindProperty] 
    public List<int> Periods { get; set; } = new();

    public List<PeriodVisualSelectResponse> ValidPeriods { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<List<PeriodVisualSelectResponse>> periodRequest =
            await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = null
            };

            return;
        }

        ValidPeriods = periodRequest.Value;

        // Get current periods linked to Offering
        // Get CourseName and OfferingName
    }
}
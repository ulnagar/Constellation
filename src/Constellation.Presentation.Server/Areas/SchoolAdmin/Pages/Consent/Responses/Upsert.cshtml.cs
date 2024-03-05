namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Consent.Responses;

using Application.ThirdPartyConsent.CreateTransaction;
using Application.ThirdPartyConsent.GetApplicationDetails;
using Application.ThirdPartyConsent.GetApplications;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;

    public UpsertModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => ConsentPages.Transactions;

    [BindProperty]
    public string StudentId { get; set; }

    [BindProperty]
    public string Submitter { get; set; }
    [BindProperty]
    public string Method { get; set; }
    [BindProperty]
    public string Notes { get; set; }

    [BindProperty]
    public List<ConsentResponse> Responses { get; set; }

    public List<ApplicationSummaryResponse> Applications { get; set; }

    public async Task OnGet()
    {
        Result<List<ApplicationSummaryResponse>> response = await _mediator.Send(new GetApplicationsQuery());

        if (response.IsSuccess)
        {
            Applications = response.Value;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        await _mediator.Send(new CreateTransactionCommand(
            StudentId,
            Submitter,
            ConsentMethod.FromValue(Method),
            Notes,
            Responses.Where(response => response.ApplicationId != Guid.Empty).ToDictionary(k => ApplicationId.FromValue(k.ApplicationId), k => k.Consent)));

        return Page();
    }

    public class ConsentResponse
    {
        public Guid ApplicationId { get; set; }
        public bool Consent { get; set; }
    }
}


namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages.Enrolment;

using Constellation.Application.Domains.StudentOnboarding.Models;
using Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplicationById;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.StudentOnboarding.Policy;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using ApplicationId = Core.Models.StudentOnboarding.Identifiers.ApplicationId;

[AllowAnonymous]
public class OfferModel : PageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public OfferModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<OfferModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ApplicantsPortal);
    }

    [FromRoute]
    public ApplicationId ApplicationId { get; set; }

    public EnrolmentApplicationResponse Application { get; set; }
    public bool Responded { get; set; }

    [BindProperty] 
    public string? CourtOrders { get; set; } = "Unset";

    [BindProperty]
    public string? OfferResponse { get; set; } = "Unset";

    [BindProperty]
    public string? HealthConditions { get; set; } = "Unset";

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        if (OfferResponse == "Unset")
        {
            ModelState.AddModelError(nameof(OfferResponse), "You must either Accept or Decline the offer of enrolment to continue.");

            await PreparePage();
            return Page();
        }

        if (OfferResponse == "Decline")
        {
            // Process Decline response

            return RedirectToPage("/Index", new { area = "Applicants", ApplicationId });
        }

        if (CourtOrders == "Unset")
        {
            ModelState.AddModelError(nameof(CourtOrders), "You must select either Yes or No to the question about court orders.");

            await PreparePage();
            return Page();
        }

        if (HealthConditions == "Unset")
        {
            ModelState.AddModelError(nameof(HealthConditions), "You must select either Yes or No to the question about health conditions.");

            await PreparePage();
            return Page();
        }

        // Process Accept response;

        return RedirectToPage("/Index", new { area = "Applicants", ApplicationId });
    }

    private async Task PreparePage()
    {
        GetEnrolmentApplicationByIdQuery command = new(ApplicationId);

        _logger
            .ForContext(nameof(GetEnrolmentApplicationByIdQuery), command, true)
            .Information("Requested to retrieve Application by user {User}", _currentUserService.UserName);

        Result<EnrolmentApplicationResponse> application = await _mediator.Send(command);

        if (application.IsFailure)
        {
            _logger
                .ForContext(nameof(GetEnrolmentApplicationByIdQuery), command, true)
                .ForContext(nameof(Error), application.Error, true)
                .Warning("Failed to retrieve Application by user {User}", _currentUserService.UserName);

            return;
        }

        if (application.Value.State != ApplicationState.PendingOfferResponse)
            Responded = true;

        Application = application.Value;
    }
}
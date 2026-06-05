namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages;

using Application.Domains.StudentOnboarding.Models;
using Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplicationById;
using Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplications;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using ApplicationId = Core.Models.StudentOnboarding.Identifiers.ApplicationId;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ApplicantsPortal);
    }
    
    [FromRoute]
    public ApplicationId ApplicationId { get; set; }

    public EnrolmentApplicationResponse Application { get; set; }

    public async Task OnGet()
    {
        var command = new GetEnrolmentApplicationByIdQuery(ApplicationId);

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

        Application = application.Value;
    }
}
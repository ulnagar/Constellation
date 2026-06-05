namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Onboarding.Enrolments;

using Application.Domains.StudentOnboarding.Models;
using Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplications;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Onboarding_Enrolments_View_Value)]
public class IndexModel : BasePageModel
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Onboarding_Enrolment_List;
    
    [ViewData]
    public string PageTitle => "Enrolments";

    public List<EnrolmentApplicationResponse> Applications { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<EnrolmentApplicationResponse>> applications = await _mediator.Send(new GetEnrolmentApplicationsQuery());

        if (applications.IsFailure)
        {
            return;
        }

        Applications = applications.Value;
    }
}
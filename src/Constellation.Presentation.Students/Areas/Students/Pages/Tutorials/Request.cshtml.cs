namespace Constellation.Presentation.Students.Areas.Students.Pages.Tutorials;

using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class RequestModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RequestModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RequestModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }
    
    [ViewData] public string ActivePage => Models.ActivePage.Tutorials;

    [BindProperty]
    public TutorialType Type { get; set; }

    [BindProperty]
    public string Subject { get; set; }

    [BindProperty]
    public List<PeriodId> Periods { get; set; }

    public async Task OnGet()
    {

    }
}
namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Domains.SciencePracs.Queries.GetLessonRollDetailsForSchoolsPortal;
using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RollModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RollModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.ScienceRolls;

    [BindProperty(SupportsGet = true)]
    public SciencePracLessonId LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    public SciencePracRollId RollId { get; set; }


    public ScienceLessonRollDetails MarkedRoll { get; set; }
    public bool Editable { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve science roll data by user {user} with Id {rollId}", _currentUserService.UserName, RollId);

        Result<ScienceLessonRollDetails> request = await _mediator.Send(new GetLessonRollDetailsForSchoolsPortalQuery(LessonId, RollId));

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        MarkedRoll = request.Value;

        if (MarkedRoll.LessonDate > DateTime.Today.AddDays(-14))
        {
            Editable = true;
        }
    }

}
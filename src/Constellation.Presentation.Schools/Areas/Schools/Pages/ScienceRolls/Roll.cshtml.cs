namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Domains.SciencePracs.Queries.GetLessonRollDetailsForSchoolsPortal;
using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_SciencePracs_View_Value)]
public class RollModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RollModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger) 
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RollModel>()
            .ForSchoolPortal();
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
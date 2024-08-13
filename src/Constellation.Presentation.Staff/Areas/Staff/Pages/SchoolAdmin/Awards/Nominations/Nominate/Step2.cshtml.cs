namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations.Nominate;

using Application.Models.Auth;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class Step2Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step2Model(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<Step1Model>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle => "New Award Nomination";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AwardNominationPeriodId PeriodId { get; set; }
    [BindProperty]
    public string? Type { get; set; }
    

    public async Task<IActionResult> OnGet()
    {
        if (Type is not null &&
            (Type.Equals(AwardType.GalaxyMedal.Value) ||
             Type.Equals(AwardType.PrincipalsAward.Value) ||
             Type.Equals(AwardType.UniversalAchiever.Value)))
        {
            return RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step4", new { area = "Staff", Type });
        }

        _logger.Information("Requested to create new Award Nomination by user {User}", _currentUserService.UserName);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(PeriodId));

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to create new Award Nomination by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));
        }


    }
}
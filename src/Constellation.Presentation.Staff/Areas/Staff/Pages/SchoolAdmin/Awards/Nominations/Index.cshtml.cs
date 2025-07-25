namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Common.PresentationModels;
using Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetAllNominationPeriods;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle => "Award Nomination Period List";

    public List<NominationPeriodResponse> Periods { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve list of Award Nomination Periods by user {User}", _currentUserService.UserName);

        Result<List<NominationPeriodResponse>> request = await _mediator.Send(new GetAllNominationPeriodsQuery(), cancellationToken);

        if (!request.IsSuccess)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Award Nomination Periods by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Periods = request.Value;
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Common.PresentationModels;
using Constellation.Application.Awards.DeleteAwardNomination;
using Constellation.Application.Awards.ExportAwardNominations;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle { get; set; } = "Award Nomination Details";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AwardNominationPeriodId PeriodId { get; set; }

    public NominationPeriodDetailResponse Period { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnGetExport(CancellationToken cancellationToken = default)
    {
        ExportAwardNominationsCommand command = new(PeriodId);

        _logger
            .ForContext(nameof(ExportAwardNominationsCommand), command, true)
            .Information("Requested to export Award Nominations by user {User}", _currentUserService.UserName);

        Result<FileDto> fileRequest = await _mediator.Send(command, cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to export Award Nominations by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(fileRequest.Error);

            await PreparePage(cancellationToken);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetDelete(
        [ModelBinder(typeof(StrongIdBinder))] AwardNominationId entryId, 
        CancellationToken cancellationToken = default)
    {
        DeleteAwardNominationCommand command = new(PeriodId, entryId);

        _logger
            .ForContext(nameof(DeleteAwardNominationCommand), command, true)
            .Information("Requested to remove Award Nomination by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command, cancellationToken);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to remove Award Nomination by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage(cancellationToken);

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve details of Award Nomination Period with id {Id} by user {User}", PeriodId, _currentUserService.UserName);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(PeriodId), cancellationToken);

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to retrieve details of Award Nomination Period with id {Id} by user {User}", PeriodId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Index", values: new { area = "Staff" }));

            return;
        }

        Period = periodRequest.Value;
        PageTitle = $"Details - {Period.Name}";
    }
}

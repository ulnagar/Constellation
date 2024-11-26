namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Awards;

using Application.Attachments.GetAttachmentFile;
using Application.Common.PresentationModels;
using Constellation.Application.Awards.GetAllAwards;
using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Application.Awards.Models;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IRecurringJobManager _jobManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IRecurringJobManager jobManager,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _jobManager = jobManager;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    public enum FilterDto
    {
        All,
        Recent,
        ThisYear
    }
    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Awards_List;
    [ViewData] public string PageTitle => "Awards List";

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Recent;

    public List<AwardResponse> Awards = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve list of Awards by user {User}", _currentUserService.UserName);

        Result<List<AwardResponse>> awardRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllAwardsQuery(false), cancellationToken),
            FilterDto.Recent => await _mediator.Send(new GetRecentAwardsQuery(20), cancellationToken),
            FilterDto.ThisYear => await _mediator.Send(new GetAllAwardsQuery(true), cancellationToken),
            _ => Result.Failure<List<AwardResponse>>(Error.NullValue)
        };

        if (awardRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), awardRequest.Error, true)
                .Warning("Failed to retrieve list of Awards by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                awardRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return;
        }

        Awards = awardRequest.Value;
    }

    public async Task<IActionResult> OnGetAttemptDownload(
        StudentAwardId id,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve Award Certificate with id {Id} by user {User}", id, _currentUserService.UserName);

        Result<AttachmentResponse> fileRequest = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, id.ToString()), cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to retrieve Award Certificate with id {Id} by user {User}", id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                fileRequest.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Awards/Index", values: new { area = "Staff" }));

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public IActionResult OnGetRefreshAwards()
    {
        _logger.Information("Requested to trigger Award Sync by user {User}", _currentUserService.UserName);

        _jobManager.Trigger(nameof(ISentralAwardSyncJob));

        return RedirectToPage("/StudentAdmin/Awards/Index", new { area = "Staff" });
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;

using Application.Attachments.GetAttachmentFile;
using Constellation.Application.Awards.GetAllAwards;
using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Application.Awards.Models;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IRecurringJobManager _jobManager;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IRecurringJobManager jobManager)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _jobManager = jobManager;
    }

    public enum FilterDto
    {
        All,
        Recent,
        ThisYear
    }
    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_List;
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Recent;

    public List<AwardResponse> Awards = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        Result<List<AwardResponse>> awardRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllAwardsQuery(false), cancellationToken),
            FilterDto.Recent => await _mediator.Send(new GetRecentAwardsQuery(20), cancellationToken),
            FilterDto.ThisYear => await _mediator.Send(new GetAllAwardsQuery(true), cancellationToken),
            _ => Result.Failure<List<AwardResponse>>(Core.Shared.Error.NullValue)
        };

        if (awardRequest.IsFailure)
        {
            Error = new()
            {
                Error = awardRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
            };

            return;
        }

        Awards = awardRequest.Value;

        return;
    }

    public async Task<IActionResult> OnGetAttemptDownload(string Id, CancellationToken cancellationToken = default)
    {
        StudentAwardId awardId = StudentAwardId.FromValue(Guid.Parse(Id));

        Result<AttachmentResponse> fileRequest = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId.ToString()), cancellationToken);

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public IActionResult OnGetRefreshAwards()
    {
        _jobManager.Trigger(nameof(ISentralAwardSyncJob));

        return RedirectToPage("/SchoolAdmin/Awards/Index", new { area = "Staff" });
    }
}

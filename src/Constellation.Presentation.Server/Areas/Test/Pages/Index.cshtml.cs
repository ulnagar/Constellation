namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.GroupTutorials;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProcessOutboxMessagesJob _job;
    private readonly IAuthService _authService;
    private readonly IRollMarkingReportJob _rollMarkingJob;
    private readonly IMediator _mediator;

    public IndexModel(
        IUnitOfWork unitOfWork,
        IProcessOutboxMessagesJob job,
        IAuthService authService,
        IRollMarkingReportJob rollMarkingJob,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _job = job;
        _authService = authService;
        _rollMarkingJob = rollMarkingJob;
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
    }

    public async Task<IActionResult> OnPostMarkRoll()
    {
        await _rollMarkingJob.StartJob(Guid.NewGuid(), default);

        return Page();
    }

    public async Task<IActionResult> OnPostProcessOutbox()
    {
        await _job.StartJob(Guid.NewGuid(), default);

        return Page();
    }

    public async Task<IActionResult> OnPostAuditParents()
    {
        await _authService.AuditParentUsers();

        return Page();
    }

}

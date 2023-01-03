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
    private readonly IMediator _mediator;

    public IndexModel(
        IUnitOfWork unitOfWork,
        IProcessOutboxMessagesJob job,
        IAuthService authService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _job = job;
        _authService = authService;
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
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

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(new TestCommand());
        return Page();
    }
}

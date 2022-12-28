namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProcessOutboxMessagesJob _job;
    private readonly IAuthService _authService;

    public IndexModel(IUnitOfWork unitOfWork, IProcessOutboxMessagesJob job, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _job = job;
        _authService = authService;
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
        return Page();
    }
}

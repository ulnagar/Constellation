namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISentralFamilyDetailsSyncJob _parentSyncJob;

    public IndexModel(
        IUnitOfWork unitOfWork,
        ISentralFamilyDetailsSyncJob parentSyncJob)
    {
        _unitOfWork = unitOfWork;
        _parentSyncJob = parentSyncJob;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
    }

    public async Task<IActionResult> OnPostSyncParents()
    {
        await _parentSyncJob.StartJob(Guid.NewGuid(), default);

        return Page();
    }
}

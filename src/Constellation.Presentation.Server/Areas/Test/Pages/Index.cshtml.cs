namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserManagerJob _job;

    public IndexModel(IUnitOfWork unitOfWork, IUserManagerJob job)
    {
        _unitOfWork = unitOfWork;
        _job = job;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _job.StartJob(Guid.NewGuid(), default);

        return Page();
    }
}

namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_unitOfWork);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return Page();
    }
}

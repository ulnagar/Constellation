using Constellation.Application.Features.MandatoryTraining.Jobs;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ISentralPhotoSyncJob _reportJob;

        public IndexModel(IUnitOfWork unitOfWork, IAuthService authService,
            ISentralPhotoSyncJob reportJob)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _reportJob = reportJob;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //await _authService.RepairStaffUserAccounts();

            await _reportJob.StartJob(new Guid(), default);

            return Page();
        }
    }
}

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        private readonly IJobDispatcherService<ISentralAwardSyncJob> _service;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, IAuthService authService, IJobDispatcherService<ISentralAwardSyncJob> service)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _authService = authService;
            _service = service;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _service.StartJob(new System.Threading.CancellationToken());

            //await _authService.RepairStaffUserAccounts();

            return Page();
        }
    }
}

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Jobs;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IMandatoryTrainingReminderJob _reportJob;
        private readonly IMediator _mediator;

        public IndexModel(IUnitOfWork unitOfWork, IAuthService authService,
            IMandatoryTrainingReminderJob reportJob, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _reportJob = reportJob;
            _mediator = mediator;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //await _authService.RepairStaffUserAccounts();

            //await _reportJob.StartJob(new Guid(), default);

            var stream = await _mediator.Send(new GenerateStaffReportCommand("615171335"));

            return File(stream.FileData, stream.FileType, stream.FileName);
        }
    }
}

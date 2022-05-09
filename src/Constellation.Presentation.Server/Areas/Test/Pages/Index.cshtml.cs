using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
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
        private readonly IJobDispatcherService<IAttendanceReportJob> _jobDispatcher;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, IJobDispatcherService<IAttendanceReportJob> jobDispatcher)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _jobDispatcher = jobDispatcher;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _jobDispatcher.StartJob(new System.Threading.CancellationToken());

            return Page();
        }
    }
}

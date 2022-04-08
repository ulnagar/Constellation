using Constellation.Application.Features.Admin.HangfireDashboard.Commands;
using Constellation.Application.Features.Admin.HangfireDashboard.Queries;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Pages
{
    [Authorize(Roles = AuthRoles.Admin)]
    public class HangfireDashboardModel : BasePageModel
    {
        private readonly IMediator _mediator;

        public HangfireDashboardModel(IMediator mediator)
        {
            JobStatuses = new List<JobActivationDto>();
            _mediator = mediator;
        }

        public ICollection<JobActivationDto> JobStatuses { get; set; }

        public class JobActivationDto
        {
            public Guid Id { get; set; }
            public string JobName { get; set; }
            public bool IsActive { get; set; }
            public DateTime? InactiveUntil { get; set; }
        }

        public async Task OnGet()
        {
            var records = await _mediator.Send(new GetJobActivatorRecordsQuery());

            foreach (var record in records)
            {
                JobStatuses.Add(new JobActivationDto
                {
                    Id = record.Id,
                    JobName = record.JobName,
                    IsActive = record.IsActive,
                    InactiveUntil = record.InactiveUntil
                });
            }

            JobStatuses.Add(new JobActivationDto
            {
                Id = Guid.Parse("ae24398b-2e82-4509-a743-ff60631215a2"),
                JobName = "IAbsenceClassworkNotificationJob"
            });
        }

        public async Task<IActionResult> OnPostToggleJob(Guid Id)
        {
            await _mediator.Send(new ToggleJobActivationCommand { Id = Id });

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRunJob(Guid Id)
        {
            await _mediator.Send(new RunJobManuallyCommand { Id = Id });

            return RedirectToPage();
        }
    }
}

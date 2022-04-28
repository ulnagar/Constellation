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
        }

        public async Task<IActionResult> OnPostToggleJob(Guid Id)
        {
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRunJob(Guid Id)
        {
            return RedirectToPage();
        }
    }
}

using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILinkShortenerGateway _gateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public IndexModel(ILinkShortenerGateway gateway, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _gateway = gateway;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        [BindProperty]
        public string LinkToShorten { get; set; }
        public string ShortLink { get; set; }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);

            await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = "451073346", Type = Absence.Whole, AgeInWeeks = 2 });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ShortLink = await _gateway.ShortenURL(LinkToShorten);

            return Page();
        }
    }
}

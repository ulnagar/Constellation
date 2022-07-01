using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
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
        private readonly ISentralGateway _sentralGateway;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, ISentralGateway sentralGateway)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _sentralGateway = sentralGateway;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var details = await _sentralGateway.GetAwardsReport();

            // Process individual students
            // Tally awards
            // Calculate expected award levels
            // Highlight discrepancies

            return Page();
        }
    }
}

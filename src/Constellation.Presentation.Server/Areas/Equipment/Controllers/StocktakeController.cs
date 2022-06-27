using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Equipment.Controllers
{
    [Area("Equipment")]
    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor, AuthRoles.User)]
    public class StocktakeController : BaseController
    {
        private readonly IMediator _mediator;

        public StocktakeController(IMediator mediator, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var stocktakes = await _mediator.Send(new GetStocktakeEventListQuery());

            var viewModel = await CreateViewModel<ListStocktakeEventsViewModel>();
            foreach (var item in stocktakes)
            {
                var entry = new ListStocktakeEventsViewModel.StocktakeEventItem
                {
                    Id = item.Id,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    Name = item.Name,
                    AcceptLateResponses = item.AcceptLateResponses
                };

                viewModel.Stocktakes.Add(entry);
            }

            return View(viewModel);
        }
    }
}

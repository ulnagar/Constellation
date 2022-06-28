using Constellation.Application.Features.Equipment.Stocktake.Commands;
using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public async Task<IActionResult> Create()
        {
            var viewModel = await CreateViewModel<UpsertStocktakeEventViewModel>();

            viewModel.Command.StartDate = DateTime.Today;
            viewModel.Command.EndDate = DateTime.Today;

            return View("Upsert", viewModel);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var viewModel = await CreateViewModel<UpsertStocktakeEventViewModel>();

            var stocktake = await _mediator.Send(new GetStocktakeEventQuery { StocktakeId = id });

            if (stocktake == null)
            {
                return RedirectToAction("Index");
            }

            viewModel.Command.Id = stocktake.Id;
            viewModel.Command.Name = stocktake.Name;
            viewModel.Command.StartDate = stocktake.StartDate;
            viewModel.Command.EndDate = stocktake.EndDate;
            viewModel.Command.AcceptLateResponses = stocktake.AcceptLateResponses;

            return View("Upsert", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpsertStocktakeEventViewModel viewModel)
        {
            if (viewModel.Command.EndDate < viewModel.Command.StartDate)
                ModelState.AddModelError("Command.EndDate", "Cannot create an event that ends before it starts!");

            if (viewModel.Command.EndDate < DateTime.Today)
                ModelState.AddModelError("Command.EndDate", "Cannot create an event that ends in the past!");

            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                return View("Upsert", viewModel);
            }

            await _mediator.Send(viewModel.Command);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var viewModel = await CreateViewModel<StocktakeEventDetailsViewModel>();

            var stocktake = await _mediator.Send(new GetStocktakeEventQuery { StocktakeId = id, IncludeSightings = true }); ;

            viewModel.Id = stocktake.Id;
            viewModel.Name = stocktake.Name;
            viewModel.StartDate = stocktake.StartDate;
            viewModel.EndDate = stocktake.EndDate;

            foreach (var reportedSighting in stocktake.Sightings)
            {
                var sighting = new StocktakeEventDetailsViewModel.Sighting
                {
                    Id = reportedSighting.Id,
                    AssetNumber = reportedSighting.AssetNumber,
                    SerialNumber = reportedSighting.SerialNumber,
                    Description = reportedSighting.Description,
                    Location = reportedSighting.LocationName,
                    User = reportedSighting.UserName,
                    SightedBy = reportedSighting.SightedBy,
                    SightedOn = reportedSighting.SightedAt
                };

                viewModel.Sightings.Add(sighting);
            }

            viewModel.TotalResponses = viewModel.Sightings.Count;
            viewModel.TotalDevices = 10000;
            viewModel.RemainingDevices = viewModel.TotalDevices - viewModel.TotalResponses;

            return View(viewModel);
        }
    }
}

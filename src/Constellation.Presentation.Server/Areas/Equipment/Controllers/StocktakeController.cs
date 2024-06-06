namespace Constellation.Presentation.Server.Areas.Equipment.Controllers;

using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Stocktake.RegisterSighting;
using Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake;
using Constellation.Presentation.Server.Helpers.Attributes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;


[Area("Equipment")]
[Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor, AuthRoles.StaffMember)]
public class StocktakeController : Controller
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterSightingCommand> _validator;

    public StocktakeController(
        IMediator mediator, 
        IUnitOfWork unitOfWork,
        IValidator<RegisterSightingCommand> validator)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
    public async Task<IActionResult> Index()
    {
        var stocktakes = await _mediator.Send(new GetStocktakeEventListQuery());

        var viewModel = new ListStocktakeEventsViewModel();
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

    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new UpsertStocktakeEventViewModel();

        viewModel.Command.StartDate = DateTime.Today;
        viewModel.Command.EndDate = DateTime.Today;

        return View("Upsert", viewModel);
    }

    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
    public async Task<IActionResult> Update(Guid id)
    {
        var viewModel = new UpsertStocktakeEventViewModel();

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
    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
    public async Task<IActionResult> Update(UpsertStocktakeEventViewModel viewModel)
    {
        if (viewModel.Command.EndDate < viewModel.Command.StartDate)
            ModelState.AddModelError("Command.EndDate", "Cannot create an event that ends before it starts!");

        if (viewModel.Command.EndDate < DateTime.Today)
            ModelState.AddModelError("Command.EndDate", "Cannot create an event that ends in the past!");

        if (!ModelState.IsValid)
        {
            return View("Upsert", viewModel);
        }

        await _mediator.Send(viewModel.Command);

        return RedirectToAction("Index");
    }

    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
    public async Task<IActionResult> Details(Guid id)
    {
        var viewModel = new StocktakeEventDetailsViewModel();

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
                SightedOn = reportedSighting.SightedAt,
                IsCancelled = reportedSighting.IsCancelled
            };

            viewModel.Sightings.Add(sighting);
        }

        viewModel.TotalResponses = viewModel.Sightings.Count;
        viewModel.TotalDevices = 10000;
        viewModel.RemainingDevices = viewModel.TotalDevices - viewModel.TotalResponses;

        return View(viewModel);
    }
}
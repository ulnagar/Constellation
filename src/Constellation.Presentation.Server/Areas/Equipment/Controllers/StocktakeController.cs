using Constellation.Application.Features.Equipment.Stocktake.Commands;
using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Features.Portal.School.Stocktake.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models;
using Constellation.Core.Models.Stocktake;
using Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Constellation.Presentation.Server.Helpers.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Equipment.Controllers
{
    [Area("Equipment")]
    [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class StocktakeController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RegisterSightedDeviceForStocktakeCommand> _validator;

        public StocktakeController(IMediator mediator, IUnitOfWork unitOfWork,
            IValidator<RegisterSightedDeviceForStocktakeCommand> validator)
            : base(unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
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

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var viewModel = await CreateViewModel<UpsertStocktakeEventViewModel>();

            viewModel.Command.StartDate = DateTime.Today;
            viewModel.Command.EndDate = DateTime.Today;

            return View("Upsert", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
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
        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
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

        [Roles(AuthRoles.Admin, AuthRoles.EquipmentEditor, AuthRoles.Editor)]
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

        public async Task<IActionResult> StaffDashboard(Guid id)
        {
            var viewModel = await CreateViewModel<StaffDashboardViewModel>();

            var staffMember = await GetStaffMember();

            var stocktake = await _mediator.Send(new GetStocktakeEventQuery { StocktakeId = id, IncludeSightings = true }); ;

            viewModel.Id = stocktake.Id;
            viewModel.Name = stocktake.Name;
            viewModel.StartDate = stocktake.StartDate;
            viewModel.EndDate = stocktake.EndDate;

            foreach (var reportedSighting in stocktake.Sightings.Where(sighting => sighting.SightedBy == staffMember.EmailAddress))
            {
                var sighting = new StaffDashboardViewModel.Sighting
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

            return View(viewModel);
        }

        public async Task<IActionResult> Submit(Guid id)
        {
            var staffMember = await GetStaffMember();

            var viewModel = await CreateViewModel<StaffStocktakeSightingViewModel>();
            viewModel.Command.StocktakeEventId = id;

            var students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery { SchoolCode = staffMember.SchoolCode });
            viewModel.Students = students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();
            var staffMembers = await _mediator.Send(new GetStaffForSelectionQuery());
            viewModel.Staff = staffMembers.OrderBy(staff => staff.LastName).ToList();
            var schools = await _mediator.Send(new GetSchoolsForSelectionQuery());
            viewModel.Schools = schools.OrderBy(school => school.Name).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(StaffStocktakeSightingViewModel viewModel)
        {
            var staffMember = await GetStaffMember();

            var result = await _validator.ValidateAsync(viewModel.Command);
            if (!result.IsValid)
            {
                result.AddToModelState(this.ModelState, "Command");
            }

            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);

                var students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery { SchoolCode = staffMember.SchoolCode });
                viewModel.Students = students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();
                var staffMembers = await _mediator.Send(new GetStaffForSelectionQuery());
                viewModel.Staff = staffMembers.OrderBy(staff => staff.LastName).ToList();
                var schools = await _mediator.Send(new GetSchoolsForSelectionQuery());
                viewModel.Schools = schools.OrderBy(school => school.Name).ToList();

                return View(viewModel);
            }

            var command = viewModel.Command;
            command.SightedAt = DateTime.Now;
            command.SightedBy = staffMember.EmailAddress;

            await _mediator.Send(command);

            return RedirectToAction("StaffDashboard", new { id = command.StocktakeEventId });
        }

        private async Task<Staff> GetStaffMember()
        {
            var username = User.Identity.Name;
            var staffUser = await _unitOfWork.Staff.FromEmailForExistCheck(username);

            return staffUser;
        }
    }
}

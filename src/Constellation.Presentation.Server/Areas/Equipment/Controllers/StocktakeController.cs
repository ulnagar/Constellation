﻿using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Stocktake.RegisterSighting;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Equipment.Models.Stocktake;
using Constellation.Presentation.Server.Helpers.Attributes;
using Constellation.Presentation.Server.Helpers.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.Equipment.Controllers
{
    using Application.Features.Equipment.Stocktake.Models;
    using Application.StaffMembers.Models;
    using Application.Students.GetStudentsFromSchoolForSelectionList;
    using Core.Shared;
    using ValidationResult = FluentValidation.Results.ValidationResult;

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

        public async Task<IActionResult> StaffDashboard(Guid id)
        {
            var viewModel = new StaffDashboardViewModel();

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
            Staff staffMember = await GetStaffMember();

            StaffStocktakeSightingViewModel viewModel = new() { StocktakeEventId = id };

            Result<List<StudentSelectionResponse>> students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(staffMember.SchoolCode));
            if (students.IsSuccess)
                viewModel.Students = students.Value.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();
            
            ICollection<StaffSelectionListResponse> staffMembers = await _mediator.Send(new GetStaffForSelectionQuery());
            viewModel.Staff = staffMembers.OrderBy(staff => staff.LastName).ToList();
            
            ICollection<PartnerSchoolForDropdownSelection> schools = await _mediator.Send(new GetSchoolsForSelectionQuery());
            viewModel.Schools = schools.OrderBy(school => school.Name).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(StaffStocktakeSightingViewModel viewModel)
        {
            Staff staffMember = await GetStaffMember();

            RegisterSightingCommand command = new(
                viewModel.StocktakeEventId,
                viewModel.SerialNumber,
                viewModel.AssetNumber,
                viewModel.Description,
                viewModel.LocationCategory,
                viewModel.LocationName,
                viewModel.LocationCode,
                viewModel.UserType,
                viewModel.UserName,
                viewModel.UserCode,
                viewModel.Comment,
                staffMember.EmailAddress,
                DateTime.Now);
            
            ValidationResult result = await _validator.ValidateAsync(command);
            if (!result.IsValid)
            {
                result.AddToModelState(this.ModelState);
            }

            if (!ModelState.IsValid)
            {
                Result<List<StudentSelectionResponse>> students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(staffMember.SchoolCode));
                if (students.IsSuccess)
                    viewModel.Students = students.Value.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();
                
                ICollection<StaffSelectionListResponse> staffMembers = await _mediator.Send(new GetStaffForSelectionQuery());
                viewModel.Staff = staffMembers.OrderBy(staff => staff.LastName).ToList();
                
                ICollection<PartnerSchoolForDropdownSelection> schools = await _mediator.Send(new GetSchoolsForSelectionQuery());
                viewModel.Schools = schools.OrderBy(school => school.Name).ToList();

                return View(viewModel);
            }
            
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

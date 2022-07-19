﻿using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.Areas.Reports.Models.Awards;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class AwardsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IRecurringJobManager _jobManager;

        public AwardsController(IMediator mediator, IUnitOfWork unitOfWork, IRecurringJobManager jobManager)
            : base(unitOfWork)
        {
            _mediator = mediator;
            _jobManager = jobManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<RecentAwardsListViewModel>();
            viewModel.Awards = await _mediator.Send(new GetRecentAwardsListQuery { RecentCount = 20 });

            return View(viewModel);
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = await CreateViewModel<DashboardViewModel>();

            return View(viewModel);
        }

        public async Task<IActionResult> GetDashboardData()
        {
            return Json(await _mediator.Send(new GetGradeAwardDataForDashboardQuery { Year = DateTime.Today.Year }));
        }

        public async Task<IActionResult> Leaderboard()
        {
            var viewModel = await CreateViewModel<AwardsChangesListViewModel>();

            var students = await _mediator.Send(new GetStudentsWithAwardQuery());

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
                var studentWinners = students.Where(student => student.CurrentGrade == grade).OrderByDescending(student => student.Awards.Count(award => award.Type == "Astra Award")).Take(5);

                foreach (var student in studentWinners)
                {
                    var entry = new AwardsChangesListViewModel.AwardRecord
                    {
                        StudentId = student.StudentId,
                        StudentName = student.DisplayName,
                        StudentGrade = student.CurrentGrade.AsName(),

                        AwardedAstras = student.Awards.Count(award => award.Type == "Astra Award")
                    };

                    entry.CalculatedStellars = Math.Floor(entry.AwardedAstras / 5);
                    entry.CalculatedGalaxies = Math.Floor(entry.AwardedAstras / 25);
                    entry.CalculatedUniversals = Math.Floor(entry.AwardedAstras / 125);

                    viewModel.Awards.Add(entry);
                }
            }

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Changes(string filter)
        {
            var viewModel = await CreateViewModel<AwardsChangesListViewModel>();

            var students = await _mediator.Send(new GetStudentsWithAwardQuery());

            foreach (var student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName))
            {
                var entry = new AwardsChangesListViewModel.AwardRecord
                {
                    StudentId = student.StudentId,
                    StudentName = student.DisplayName,
                    StudentGrade = student.CurrentGrade.AsName(),

                    AwardedAstras = student.Awards.Count(award => award.Type == "Astra Award"),
                    AwardedStellars = student.Awards.Count(award => award.Type == "Stellar Award"),
                    AwardedGalaxies = student.Awards.Count(award => award.Type == "Galaxy Medal"),
                    AwardedUniversals = student.Awards.Count(award => award.Type == "Aurora Universal Achiever")
                };

                entry.CalculatedStellars = Math.Floor(entry.AwardedAstras / 5);
                entry.CalculatedGalaxies = Math.Floor(entry.AwardedAstras / 25);
                entry.CalculatedUniversals = Math.Floor(entry.AwardedAstras / 125);

                if (filter == "Additions")
                {
                    if (entry.AwardedStellars < entry.CalculatedStellars ||
                    entry.AwardedGalaxies < entry.CalculatedGalaxies ||
                    entry.AwardedUniversals < entry.CalculatedUniversals)
                        viewModel.Awards.Add(entry);
                } else if (filter == "Overages")
                {
                    if (entry.AwardedStellars > entry.CalculatedStellars ||
                    entry.AwardedGalaxies > entry.CalculatedGalaxies ||
                    entry.AwardedUniversals > entry.CalculatedUniversals)
                        viewModel.Awards.Add(entry);
                } else if (string.IsNullOrWhiteSpace(filter))
                {
                    if (entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                        viewModel.Awards.Add(entry);
                }
            }

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public IActionResult Update()
        {
            _jobManager.Trigger(nameof(ISentralAwardSyncJob));

            return RedirectToAction("Index");
        }
    }
}

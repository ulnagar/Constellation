namespace Constellation.Presentation.Server.Areas.Reports.Controllers;

using Constellation.Application.Awards.GetAwardCountsByTypeByGrade;
using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetCurrentStudentsWithAwards;
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

[Area("Reports")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class AwardsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IRecurringJobManager _jobManager;

    public AwardsController(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IRecurringJobManager jobManager)
        : base(unitOfWork)
    {
        _mediator = mediator;
        _jobManager = jobManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var viewModel = await CreateViewModel<DashboardViewModel>();

        return View(viewModel);
    }

    public async Task<IActionResult> GetDashboardData(CancellationToken cancellationToken = default)
    {
        var request = await _mediator.Send(new GetAwardCountsByTypeByGradeQuery(DateTime.Today.Year), cancellationToken);

        return Json(request.Value);
    }

    public async Task<IActionResult> Leaderboard(CancellationToken cancellationToken = default)
    {
        var viewModel = await CreateViewModel<AwardsChangesListViewModel>();

        viewModel.IsFiltered = false;
        viewModel.StartDate = DateTime.Today;
        viewModel.EndDate = DateTime.Today;

        var request = await _mediator.Send(new GetCurrentStudentsWithAwardsQuery(), cancellationToken);

        var students = request.Value;

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            var studentWinners = students.Where(student => student.Grade == grade).OrderByDescending(student => student.Awards.Count(award => award.Type == "Astra Award")).Take(5);

            foreach (var student in studentWinners)
            {
                var entry = new AwardsChangesListViewModel.AwardRecord
                {
                    StudentId = student.StudentId,
                    StudentName = student.DisplayName,
                    StudentGrade = student.Grade.AsName(),

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leaderboard(AwardsChangesListViewModel viewModel, CancellationToken cancellationToken = default)
    {
        await UpdateViewModel(viewModel);
        viewModel.IsFiltered = true;

        var request = await _mediator.Send(new GetCurrentStudentsWithAwardsQuery(), cancellationToken);

        var students = request.Value;

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            var studentWinners = students
                .Where(student => student.Grade == grade)
                .OrderByDescending(student => student.Awards
                    .Count(award => award.Type == "Astra Award" && award.AwardedOn.Date >= viewModel.StartDate && award.AwardedOn.Date <= viewModel.EndDate))
                .Take(5);

            foreach (var student in studentWinners)
            {
                var entry = new AwardsChangesListViewModel.AwardRecord
                {
                    StudentId = student.StudentId,
                    StudentName = student.DisplayName,
                    StudentGrade = student.Grade.AsName(),

                    AwardedAstras = student.Awards.Count(award => award.Type == "Astra Award" && award.AwardedOn.Date >= viewModel.StartDate && award.AwardedOn.Date <= viewModel.EndDate)
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
    public async Task<IActionResult> Changes(string filter, CancellationToken cancellationToken = default)
    {
        var viewModel = await CreateViewModel<AwardsChangesListViewModel>();

        var request = await _mediator.Send(new GetCurrentStudentsWithAwardsQuery(), cancellationToken);

        var students = request.Value;

        foreach (var student in students.OrderBy(student => student.Grade).ThenBy(student => student.LastName))
        {
            var entry = new AwardsChangesListViewModel.AwardRecord
            {
                StudentId = student.StudentId,
                StudentName = student.DisplayName,
                StudentGrade = student.Grade.AsName(),

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

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;

using Application.Awards.Enums;
using Application.Students.AuditAwardTallyValues;
using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class ChangesModel : BasePageModel
{
    private readonly IMediator _mediator;

    public ChangesModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Changes;

    [BindProperty(SupportsGet = true)]
    public AwardsFilter Filter { get; set; } = AwardsFilter.All;

    public List<StudentAwardStatisticsResponse> AwardStatistics { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        Result<List<StudentAwardStatisticsResponse>> statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            // Do something
        }

        AwardStatistics = Filter switch
        {
            AwardsFilter.All => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                .ToList(),
            AwardsFilter.Additions => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars < entry.CalculatedStellars ||
                    entry.AwardedGalaxies < entry.CalculatedGalaxies ||
                    entry.AwardedUniversals < entry.CalculatedUniversals)
                .ToList(),
            AwardsFilter.Overages => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars > entry.CalculatedStellars ||
                    entry.AwardedGalaxies > entry.CalculatedGalaxies ||
                    entry.AwardedUniversals > entry.CalculatedUniversals)
                .ToList(),
            _ => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                .ToList()
        };
    }

    public async Task<IActionResult> OnGetAuditValues()
    {
        await _mediator.Send(new AuditAwardTallyValuesCommand());

        return RedirectToPage();
    }
}

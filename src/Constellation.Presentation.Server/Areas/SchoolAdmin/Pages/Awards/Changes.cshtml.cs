namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards;

using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
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

    public enum FilterDto
    {
        All,
        Additions,
        Overages
    }

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.All;

    public List<StudentAwardStatisticsResponse> AwardStatistics { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        ViewData["ActivePage"] = "Changes";

        await GetClasses(_mediator);

        var statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            // Do something
        }

        AwardStatistics = Filter switch
        {
            FilterDto.All => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                .ToList(),
            FilterDto.Additions => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars < entry.CalculatedStellars ||
                    entry.AwardedGalaxies < entry.CalculatedGalaxies ||
                    entry.AwardedUniversals < entry.CalculatedUniversals)
                .ToList(),
            FilterDto.Overages => statisticsRequest.Value.Where(entry =>
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
}

namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Application.Interfaces.Gateways;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISentralGateway _gateway;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISentralGateway gateway,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _gateway = gateway;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {
        List<ValidAttendenceReportDate> result = await _gateway.GetTermsAndWeeksFromApi("2025");

        var thisWeek = result.FirstOrDefault(entry => entry.Description == "Term 3 Week 8");

        var index = result.IndexOf(thisWeek);
        index = index + 10;

        if (index > (result.Count - 1))
            index = result.Count - 1;

        var tenWeeks = result[index];
    }

}
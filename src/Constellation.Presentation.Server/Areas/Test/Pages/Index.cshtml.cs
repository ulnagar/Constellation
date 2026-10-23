namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Extensions;
using Application.Interfaces.Gateways;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ISentralGateway _sentralGateway;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ISentralGateway sentralGateway,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _sentralGateway = sentralGateway;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => "";
    
    public async Task OnGet()
    {
        // Create a recurring set:

        //var result = await _sentralGateway.AddStudentUnavailabilities(
        //    "2897",
        //    [
        //        new DateOnly(2026, 10, 19),
        //        new DateOnly(2026, 11, 2),
        //        new DateOnly(2026, 11, 16)
        //    ],
        //    new TimeOnly(9, 0, 0),
        //    new TimeOnly(10, 0, 0));

        // Delete between dates:

        //var result = await _sentralGateway.RemoveStudentUnavailabilities(
        //    "2897",
        //    new DateOnly(2026, 10, 19),
        //    new DateOnly(2026, 11, 16));

        var result = await _sentralGateway.GetTermsAndWeeksFromApi("2026");

        foreach (var entry in result)
        {
            entry.StartDate.GetDayNumber();
        }

        var activePage = result;
    }

}
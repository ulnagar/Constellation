namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using Application.Domains.LinkedSystems.Canvas.Models;
using Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Canvas.Models;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICanvasGateway _canvasGateway;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICanvasGateway canvasGateway,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _canvasGateway = canvasGateway;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => "";
    
    public async Task OnGet()
    {
        await _mediator.Send(new GetAwardDetailsFromSentralQuery());
    }

}
namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using Application.Domains.LinkedSystems.Canvas.Models;
using Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;
using Application.Domains.Messaging.Sms.Commands.CreateNewIncomingSmsRecord;
using Application.Domains.Messaging.Sms.Dtos;
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
        IncomingSms sms = new()
        {
            MsgId = "846122192",
            From = "61417243239",
            To = "61400896896",
            Msg = "0709 we received email that that class was optional as its exam period",
            Date = new DateTimeOffset(2026, 9, 8, 19, 23, 23, new TimeSpan(10, 0, 0))
        };

        CreateNewIncomingSmsRecordCommand command = new(sms);

        await _mediator.Send(command);
    }

}
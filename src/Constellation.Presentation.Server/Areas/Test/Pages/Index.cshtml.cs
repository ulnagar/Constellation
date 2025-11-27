namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Contacts.Interfaces;
using Application.Interfaces.Gateways;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISentralGateway _sentralGateway;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISentralGateway sentralGateway,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _sentralGateway = sentralGateway;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {
        await _sentralGateway.GetAbsencesFromApi();
    }

}
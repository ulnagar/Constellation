namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Contacts.Interfaces;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IStudentFlagCacheService _flagCache;
    private readonly IEmergencyService _emergencyService;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IEmergencyService emergencyService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _emergencyService = emergencyService;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {
        EventId eventId = EventId.FromValue(new Guid("5e7ddc03-2eae-498d-a4d9-a7002ea239aa"));

        await _emergencyService.SendEmergencyAlerts(eventId);
    }

}
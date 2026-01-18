namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Contacts.Interfaces;
using Application.Models.Auth;
using BaseModels;
using Constellation.Infrastructure.Persistence.ConstellationContext.Outbox;
using Core.Abstractions.Services;
using Core.IntegrationEvents;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public OutboxMessage Message { get; set; }

    public async Task OnGet()
    {
        var integrationEvent = new AppIdentityCodeUpdatedIntegrationEvent(new());

        Message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOn = integrationEvent.DelayUntil?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(
                integrationEvent,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })
        };
    }
}
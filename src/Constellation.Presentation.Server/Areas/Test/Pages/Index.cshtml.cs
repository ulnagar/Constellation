namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Constellation.Application.Interfaces.Gateways;
using Core.Abstractions.Services;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ISentralGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _gateway = gateway;
        _logger = logger;
    }

    public async Task OnGet()
    {
        await _gateway.GetEnrolledDatesForStudent("1277", "2023", new DateOnly(2023, 01, 23), new DateOnly(2023, 7, 1));
    }
}
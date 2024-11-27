namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Models.Auth;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task OnGet()
    {

    }
}
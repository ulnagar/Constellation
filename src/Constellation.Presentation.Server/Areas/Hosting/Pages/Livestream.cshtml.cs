namespace Constellation.Presentation.Server.Areas.Hosting.Pages;

using Application.Domains.Hosting.Queries.GetLivestream;
using Constellation.Application.Domains.Hosting.Queries.GetNewsletter;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public sealed class LivestreamModel : PageModel
{
    private readonly ISender _mediator;

    public LivestreamModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string EmbedCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Result<Livestream> result = await _mediator.Send(new GetLivestreamQuery(Id));

        if (result.IsSuccess)
        {
            EmbedCode = result.Value.EmbedCode;
            Description = result.Value.Description;
        }
    }
}


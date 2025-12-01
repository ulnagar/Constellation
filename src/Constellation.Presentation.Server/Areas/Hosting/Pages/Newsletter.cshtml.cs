namespace Constellation.Presentation.Server.Areas.Hosting.Pages;

using Constellation.Application.Domains.Hosting.Queries.GetNewsletter;
using Constellation.Core.Models.Hosting;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public sealed class NewsletterModel : PageModel
{
    private readonly ISender _mediator;

    public NewsletterModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public int Issue { get; set; }

    public string EmbedCode { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Result<Newsletter> newsletterResult = await _mediator.Send(new GetNewsletterQuery(Issue));

        if (newsletterResult.IsSuccess)
        {
            EmbedCode = newsletterResult.Value.EmbedCode;
        }
    }
}


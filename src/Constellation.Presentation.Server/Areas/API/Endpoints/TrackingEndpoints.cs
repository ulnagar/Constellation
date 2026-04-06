namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Interfaces.Services;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Tracking;

public static class TrackingEndpoints
{
    private static readonly byte[] _transparentGif = 
        Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

    public static void MapTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("track/open/{emailMessageId}", HandleTrackingPixel)
            .WithName("TrackingPixel");

        app.MapGet("track/click/{emailMessageId}", HandleTrackingLink)
            .WithName("TrackingLink");
    }

    private static async Task<IResult> HandleTrackingLink(
        EmailId emailMessageId,
        string? url,
        ITrackingEventQueueService queue,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Results.BadRequest();

        var destination = Uri.UnescapeDataString(url);

        if (!Uri.TryCreate(destination, UriKind.Absolute, out var destinationUri)
            || (destinationUri.Scheme != Uri.UriSchemeHttp
                && destinationUri.Scheme != Uri.UriSchemeHttps))
            return Results.BadRequest();

        await queue.EnqueueAsync(new EmailClickEvent(emailMessageId, destination)
        {
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = context.Request.Headers.UserAgent.ToString()
        });

        return Results.Redirect(destination, permanent: false);
    }

    private static async Task<IResult> HandleTrackingPixel(
        EmailId emailMessageId,
        ITrackingEventQueueService queue,
        HttpContext context)
    {
        await queue.EnqueueAsync(new EmailOpenEvent(emailMessageId)
        {
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = context.Request.Headers.UserAgent.ToString()
        });
        
        return Results.File(_transparentGif, "image/gif");
    }
}
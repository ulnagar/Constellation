namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Interfaces.Services;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Tracking;
using Microsoft.AspNetCore.Mvc;

public static class TrackingEndpoints
{
    private static readonly byte[] _transparentGif = 
        Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

    public static void MapTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("track/open/{emailMessageId}", HandleTrackingPixel)
            .WithName("TrackingPixel");
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
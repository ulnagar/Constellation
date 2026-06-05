namespace Constellation.Presentation.Applicants.Helpers.Routing;

using Application.Domains.StudentOnboarding.Queries.DoesApplicationIdExist;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using ApplicationId = Core.Models.StudentOnboarding.Identifiers.ApplicationId;

public sealed class ApplicantValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ApplicantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISender mediator, IMemoryCache cache)
    {
        RouteValueDictionary routeValues = context.GetRouteData().Values;
        
        // ensure that only pages in the Applicants area run this code
        bool isApplicantsArea = context.Request.Path.StartsWithSegments("/Applicants", StringComparison.OrdinalIgnoreCase);

        // ensure that any pages in the /Applicants/Error folder do not run this code
        bool isErrorPage = routeValues.TryGetValue("page", out var page)
            && page?.ToString().StartsWith("/Error/", StringComparison.OrdinalIgnoreCase) == true;
        
        if (isApplicantsArea
            && !isErrorPage
            && routeValues.TryGetValue("applicationId", out var idValue)
            && Guid.TryParse(idValue?.ToString(), out var applicationGuid))
        {
            ApplicationId applicationId = ApplicationId.FromValue(applicationGuid);

            Result<bool>? result = await cache.GetOrCreateAsync($"application:{applicationId}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30);
                return await mediator.Send(new DoesApplicationIdExistQuery(applicationId));
            });

            if (result is null || result.IsFailure || !result.Value)
            {
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                context.Response.Headers.Location = "/Error";
                return;
            }
        }

        await _next(context);
    }
}


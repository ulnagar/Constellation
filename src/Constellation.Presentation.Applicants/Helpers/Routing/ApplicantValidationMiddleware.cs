namespace Constellation.Presentation.Applicants.Helpers.Routing;

using Application.Domains.StudentOnboarding.Queries.DoesApplicantIdExist;
using Constellation.Core.Models.StudentOnboarding;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Core.Models.StudentOnboarding.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

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
        
        if (routeValues.TryGetValue("applicantId", out var idValue)
            && Guid.TryParse(idValue?.ToString(), out var applicantGuid))
        {
            ApplicantId applicantId = ApplicantId.FromValue(applicantGuid);

            Result<bool>? result = await cache.GetOrCreateAsync($"applicant:{applicantId}", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30);
                return await mediator.Send(new DoesApplicantIdExistQuery(applicantId));
            });

            if (result is null || result.IsFailure || !result.Value)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }

        await _next(context);
    }
}


namespace Constellation.Presentation.Shared.Extensions;

using System;
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        string? id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("User ID claim is missing.");
        
        bool success = Guid.TryParse(id, out Guid userId);

        if (success)
            return userId;

        throw new InvalidOperationException("User ID claim is invalid.");
    }
}
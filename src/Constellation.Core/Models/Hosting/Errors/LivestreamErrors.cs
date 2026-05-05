namespace Constellation.Core.Models.Hosting.Errors;

using Shared;

public static class LivestreamErrors
{
    public static readonly Func<Guid, Error> NotFound = id => new(
        "Hosting.Livestream.NotFound",
        $"Could not find a Livestream with the id {id}");

    public static readonly Error MustIncludeName = new(
        "Hosting.Livestream.MustIncludeName",
        "Livestream must include a valid Name.");

    public static readonly Error MustIncludeEmbedCode = new(
        "Hosting.Livestream.MustIncludeEmbedCode",
        "Livestream must include a valid Embed Code.");

    public static readonly Error InvalidExpiryDate = new(
        "Hosting.Livestream.InvalidExpiryDate",
        "Livestream expiry date must be after start date");

    public static readonly Error ExpiryDateMustBeInTheFuture = new(
        "Hosting.Livestream.ExpiryDateMustBeInTheFuture",
        "Livestream expiry date must be in the future");
}
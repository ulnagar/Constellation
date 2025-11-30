namespace Constellation.Core.Models.Hosting.Errors;

using Constellation.Core.Shared;
using System;

public static class NewsletterErrors
{
    public static readonly Func<int, Error> NotFound = issue => new(
        "Hosting.Newsletters.NotFound",
        $"Could not find Newsletter with Issue number {issue}.");

    public static readonly Error MustIncludeName = new(
        "Hosting.Newsletters.MustIncludeName",
        "Newsletter must include a valid Name.");

    public static readonly Error MustIncludeEmbedCode = new(
        "Hosting.Newsletters.MustIncludeEmbedCode",
        "Newsletter must include a valid Embed Code.");
}

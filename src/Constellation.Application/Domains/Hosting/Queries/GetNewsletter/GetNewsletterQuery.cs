namespace Constellation.Application.Domains.Hosting.Queries.GetNewsletter;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetNewsletterQuery(
    int Issue)
    : IQuery<string>;
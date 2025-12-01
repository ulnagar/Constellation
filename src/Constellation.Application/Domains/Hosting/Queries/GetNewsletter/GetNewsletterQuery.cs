namespace Constellation.Application.Domains.Hosting.Queries.GetNewsletter;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Hosting;

public sealed record GetNewsletterQuery(
    int Issue)
    : IQuery<Newsletter>;
namespace Constellation.Application.Domains.Hosting.Queries.GetAllNewsletters;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Hosting;
using System.Collections.Generic;

public sealed record GetAllNewslettersQuery()
    : IQuery<List<Newsletter>>;

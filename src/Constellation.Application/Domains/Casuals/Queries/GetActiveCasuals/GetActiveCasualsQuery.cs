namespace Constellation.Application.Domains.Casuals.Queries.GetActiveCasuals;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetActiveCasualsQuery()
    : IQuery<List<CasualsListResponse>>;

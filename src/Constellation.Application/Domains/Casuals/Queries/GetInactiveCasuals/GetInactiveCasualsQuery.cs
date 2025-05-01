namespace Constellation.Application.Domains.Casuals.Queries.GetInactiveCasuals;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetInactiveCasualsQuery()
    : IQuery<List<CasualsListResponse>>;
namespace Constellation.Application.Domains.Casuals.Queries.GetAllCasuals;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllCasualsQuery()
    : IQuery<List<CasualsListResponse>>;

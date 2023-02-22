namespace Constellation.Application.Casuals.GetActiveCasuals;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Casuals.Models;
using System.Collections.Generic;

public sealed record GetActiveCasualsQuery()
    : IQuery<List<CasualsListResponse>>;

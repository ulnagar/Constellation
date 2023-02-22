namespace Constellation.Application.Casuals.GetInactiveCasuals;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Casuals.Models;
using System.Collections.Generic;

public sealed record GetInactiveCasualsQuery()
    : IQuery<List<CasualsListResponse>>;
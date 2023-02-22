namespace Constellation.Application.Casuals.GetAllCasuals;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Casuals.Models;
using System.Collections.Generic;

public sealed record GetAllCasualsQuery()
    : IQuery<List<CasualsListResponse>>;

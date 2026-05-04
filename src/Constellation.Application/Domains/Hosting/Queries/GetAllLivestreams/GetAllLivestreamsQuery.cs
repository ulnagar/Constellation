namespace Constellation.Application.Domains.Hosting.Queries.GetAllLivestreams;

using Abstractions.Messaging;
using Core.Models.Hosting;
using System.Collections.Generic;

public sealed record GetAllLivestreamsQuery()
    :IQuery<List<Livestream>>;
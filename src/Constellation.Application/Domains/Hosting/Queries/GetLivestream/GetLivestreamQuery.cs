namespace Constellation.Application.Domains.Hosting.Queries.GetLivestream;

using Abstractions.Messaging;
using Core.Models.Hosting;
using System;

public sealed record GetLivestreamQuery(
    Guid Id)
    : IQuery<Livestream>;
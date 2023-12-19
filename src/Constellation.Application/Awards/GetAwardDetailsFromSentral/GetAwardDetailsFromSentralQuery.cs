namespace Constellation.Application.Awards.GetAwardDetailsFromSentral;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAwardDetailsFromSentralQuery()
    : IQuery<List<AwardDetailResponse>>;
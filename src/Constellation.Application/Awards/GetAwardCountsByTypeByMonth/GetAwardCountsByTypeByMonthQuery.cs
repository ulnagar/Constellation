namespace Constellation.Application.Awards.GetAwardCountsByTypeByMonth;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAwardCountsByTypeByMonthQuery(
    int Months)
    : IQuery<List<AwardCountByTypeByMonthResponse>>;
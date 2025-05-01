namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardCountsByTypeByGrade;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAwardCountsByTypeByGradeQuery(
    int Year)
    : IQuery<List<AwardCountByTypeByGradeResponse>>;
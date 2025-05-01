namespace Constellation.Application.Domains.Enrolments.Queries.GetFTETotalByGrade;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFTETotalByGradeQuery()
    : IQuery<List<GradeFTESummaryResponse>>;
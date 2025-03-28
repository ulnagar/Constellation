namespace Constellation.Application.OfferingEnrolments.GetFTETotalByGrade;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFTETotalByGradeQuery()
    : IQuery<List<GradeFTESummaryResponse>>;
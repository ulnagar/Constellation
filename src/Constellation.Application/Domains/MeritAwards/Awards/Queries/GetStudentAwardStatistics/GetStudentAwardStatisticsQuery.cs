namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetStudentAwardStatisticsQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IQuery<List<StudentAwardStatisticsResponse>>;
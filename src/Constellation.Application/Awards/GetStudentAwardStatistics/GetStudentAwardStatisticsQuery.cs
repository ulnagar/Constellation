namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentAwardStatisticsQuery()
    : IQuery<List<StudentAwardStatisticsResponse>>;
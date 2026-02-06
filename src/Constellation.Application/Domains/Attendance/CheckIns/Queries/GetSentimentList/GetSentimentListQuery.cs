namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetSentimentList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSentimentListQuery()
    : IQuery<List<string>>;
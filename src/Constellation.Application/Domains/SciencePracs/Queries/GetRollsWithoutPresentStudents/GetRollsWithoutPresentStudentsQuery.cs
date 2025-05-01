namespace Constellation.Application.Domains.SciencePracs.Queries.GetRollsWithoutPresentStudents;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetRollsWithoutPresentStudentsQuery
    : IQuery<List<NotPresentRollResponse>>;
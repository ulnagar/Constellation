namespace Constellation.Application.SciencePracs.GetRollsWithoutPresentStudents;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetRollsWithoutPresentStudentsQuery
    : IQuery<List<NotPresentRollResponse>>;
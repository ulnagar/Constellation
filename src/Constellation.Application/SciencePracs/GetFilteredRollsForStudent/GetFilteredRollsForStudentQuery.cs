namespace Constellation.Application.SciencePracs.GetFilteredRollsForStudent;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetFilteredRollsForStudentQuery(
        string StudentId)
    : IQuery<List<RollSummaryResponse>>;
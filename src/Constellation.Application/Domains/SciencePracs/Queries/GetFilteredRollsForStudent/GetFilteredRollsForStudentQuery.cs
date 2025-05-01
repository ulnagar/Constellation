namespace Constellation.Application.Domains.SciencePracs.Queries.GetFilteredRollsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetFilteredRollsForStudentQuery(
        StudentId StudentId)
    : IQuery<List<RollSummaryResponse>>;
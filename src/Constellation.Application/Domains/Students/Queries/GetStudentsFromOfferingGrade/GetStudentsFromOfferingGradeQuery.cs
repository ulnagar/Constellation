namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromOfferingGrade;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetStudentsFromOfferingGradeQuery(
    OfferingId OfferingId)
    : IQuery<List<StudentFromGradeResponse>>;
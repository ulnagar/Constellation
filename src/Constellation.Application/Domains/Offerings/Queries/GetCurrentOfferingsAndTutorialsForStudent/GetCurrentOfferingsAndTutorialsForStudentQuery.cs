namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsAndTutorialsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentOfferingsAndTutorialsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<DetailResponse>>;
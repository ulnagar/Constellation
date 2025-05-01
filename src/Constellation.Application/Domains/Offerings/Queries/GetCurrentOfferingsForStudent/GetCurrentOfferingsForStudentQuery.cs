namespace Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentOfferingsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<OfferingDetailResponse>>;
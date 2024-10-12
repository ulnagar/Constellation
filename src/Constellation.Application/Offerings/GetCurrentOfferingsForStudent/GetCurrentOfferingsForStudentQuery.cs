namespace Constellation.Application.Offerings.GetCurrentOfferingsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentOfferingsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<OfferingDetailResponse>>;
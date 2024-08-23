namespace Constellation.Application.Offerings.GetCurrentOfferingsForStudent;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentOfferingsForStudentQuery(
    string StudentId)
    : IQuery<List<OfferingDetailResponse>>;
namespace Constellation.Application.Students.GetCurrentStudentsWithCurrentOfferings;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithCurrentOfferingsQuery
    : IQuery<List<StudentWithOfferingsResponse>>;
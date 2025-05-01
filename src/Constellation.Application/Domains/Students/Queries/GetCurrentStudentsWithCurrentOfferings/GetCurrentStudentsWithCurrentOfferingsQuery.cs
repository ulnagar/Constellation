namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithCurrentOfferings;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithCurrentOfferingsQuery
    : IQuery<List<StudentWithOfferingsResponse>>;
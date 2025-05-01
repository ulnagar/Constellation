namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsFromList;

using Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;

public sealed record GetSchoolsFromListQuery(
    List<string> SchoolCodes)
    : IQuery<List<SchoolDto>>;
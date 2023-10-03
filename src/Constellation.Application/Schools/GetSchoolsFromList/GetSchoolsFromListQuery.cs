namespace Constellation.Application.Schools.GetSchoolsFromList;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using System.Collections.Generic;

public sealed record GetSchoolsFromListQuery(
    List<string> SchoolCodes)
    : IQuery<List<SchoolDto>>;
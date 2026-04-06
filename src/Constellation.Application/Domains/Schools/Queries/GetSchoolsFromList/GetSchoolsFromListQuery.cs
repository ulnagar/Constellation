namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsFromList;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using DTOs;
using System.Collections.Generic;

public sealed record GetSchoolsFromListQuery(
    List<SchoolCode> SchoolCodes)
    : IQuery<List<SchoolDto>>;
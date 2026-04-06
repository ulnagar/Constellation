namespace Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolCodes;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentPartnerSchoolCodesQuery()
    : IQuery<List<SchoolCode>>;
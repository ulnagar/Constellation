namespace Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolCodes;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentPartnerSchoolCodesQuery()
    : IQuery<List<string>>;
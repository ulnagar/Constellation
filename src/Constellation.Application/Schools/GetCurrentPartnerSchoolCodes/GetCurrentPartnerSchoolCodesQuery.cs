namespace Constellation.Application.Schools.GetCurrentPartnerSchoolCodes;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentPartnerSchoolCodesQuery()
    : IQuery<List<string>>;
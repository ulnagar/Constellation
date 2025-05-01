namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentsWithFilter;

using Core.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System;

public sealed record ConsentSummaryResponse(
    Name Student,
    Grade Grade,
    string School,
    ConsentId ConsentId,
    DateOnly ReceivedOn,
    string ApplicationName,
    bool ConsentProvided);

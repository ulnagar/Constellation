namespace Constellation.Presentation.Staff.Shared.Models;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record ClassRecord(
    OfferingId Id,
    string Name,
    string Teacher,
    string Grade);
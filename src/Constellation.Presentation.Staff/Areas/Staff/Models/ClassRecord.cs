namespace Constellation.Presentation.Staff.Areas.Staff.Models;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record ClassRecord(
    OfferingId Id,
    string Name,
    string Teacher,
    string Grade);
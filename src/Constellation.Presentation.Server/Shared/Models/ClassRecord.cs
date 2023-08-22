namespace Constellation.Presentation.Server.Shared.Models;

using Constellation.Core.Models.Subjects.Identifiers;

public sealed record ClassRecord(
    OfferingId Id,
    string Name,
    string Teacher,
    string Grade);
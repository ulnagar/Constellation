namespace Constellation.Application.Domains.Families.Models;

using Constellation.Core.Models.Identifiers;

public sealed record ParentResponse(
    ParentId ParentId,
    string ParentName);


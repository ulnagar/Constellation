namespace Constellation.Application.Domains.LinkedSystems.Teams.Models;

using System;

public sealed record TeamResource(
    Guid Id,
    string Name,
    string Description,
    string Link,
    bool Archived);
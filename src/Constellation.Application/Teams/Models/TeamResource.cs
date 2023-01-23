namespace Constellation.Application.Teams.Models;

using System;

public sealed record TeamResource(
    Guid Id,
    string Name,
    string Description,
    string Link,
    bool Archived);
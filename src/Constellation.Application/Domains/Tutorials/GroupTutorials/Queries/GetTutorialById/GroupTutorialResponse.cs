namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Queries.GetTutorialById;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record GroupTutorialResponse(
    GroupTutorialId Id, 
    string Name, 
    DateOnly StartDate, 
    DateOnly EndDate);

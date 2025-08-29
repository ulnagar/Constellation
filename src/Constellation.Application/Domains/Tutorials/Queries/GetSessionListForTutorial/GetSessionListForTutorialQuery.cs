namespace Constellation.Application.Domains.Tutorials.Queries.GetSessionListForTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionListForTutorialQuery(
    TutorialId TutorialId)
    : IQuery<List<SessionListResponse>>;
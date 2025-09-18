namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetAllTutorialRequests;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllTutorialRequestsQuery()
    : IQuery<List<TutorialRequestSummaryResponse>>;

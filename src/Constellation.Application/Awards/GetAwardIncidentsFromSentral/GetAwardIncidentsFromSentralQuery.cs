namespace Constellation.Application.Awards.GetAwardIncidentsFromSentral;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAwardIncidentsFromSentralQuery(
    string StudentId,
    string Year)
    : IQuery<List<AwardIncidentResponse>>;
namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetProposedTutorialNameForRequest;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;

public sealed record GetProposedTutorialNameForRequestQuery(
    RequestId RequestId)
    : IQuery<TutorialName>;
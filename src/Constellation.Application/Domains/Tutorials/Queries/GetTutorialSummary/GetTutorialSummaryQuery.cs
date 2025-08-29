namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialSummary;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record GetTutorialSummaryQuery(
    TutorialId TutorialId)
    : IQuery<TutorialSummaryResponse>;
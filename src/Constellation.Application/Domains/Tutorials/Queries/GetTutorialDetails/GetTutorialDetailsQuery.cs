namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialDetails;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record GetTutorialDetailsQuery(
    TutorialId Id)
    : IQuery<TutorialDetailsResponse>;
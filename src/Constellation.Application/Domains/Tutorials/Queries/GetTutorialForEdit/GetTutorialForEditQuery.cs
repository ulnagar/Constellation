namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialForEdit;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record GetTutorialForEditQuery(
    TutorialId TutorialId)
    : IQuery<TutorialForEditResponse>;
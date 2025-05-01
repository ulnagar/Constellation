namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetTutorialByIdQuery(
    GroupTutorialId TutorialId) 
    : IQuery<GroupTutorialResponse>;

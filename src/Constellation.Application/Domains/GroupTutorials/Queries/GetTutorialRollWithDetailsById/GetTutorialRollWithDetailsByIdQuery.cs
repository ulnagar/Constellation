namespace Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialRollWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetTutorialRollWithDetailsByIdQuery(
    GroupTutorialId TutorialId,
    TutorialRollId RollId) 
    : IQuery<TutorialRollDetailResponse>;

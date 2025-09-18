namespace Constellation.Application.Domains.Tutorials.GroupTutorials.Queries.GetTutorialWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetTutorialWithDetailsByIdQuery(
    GroupTutorialId Id) 
    : IQuery<GroupTutorialDetailResponse>;

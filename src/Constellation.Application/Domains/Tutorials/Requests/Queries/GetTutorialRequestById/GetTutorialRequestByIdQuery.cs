namespace Constellation.Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;

public sealed record GetTutorialRequestByIdQuery(
    RequestId RequestId)
    : IQuery<TutorialRequestDetailsResponse>;

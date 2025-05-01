namespace Constellation.Application.Domains.Casuals.Queries.GetCasualById;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetCasualByIdQuery(
    CasualId CasualId)
    : IQuery<CasualResponse>;

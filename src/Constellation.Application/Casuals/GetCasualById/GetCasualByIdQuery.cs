namespace Constellation.Application.Casuals.GetCasualById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetCasualByIdQuery(
    CasualId CasualId)
    : IQuery<CasualResponse>;

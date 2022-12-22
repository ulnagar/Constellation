namespace Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record GetTutorialRollWithDetailsByIdQuery(
    Guid TutorialId,
    Guid RollId) : IQuery<TutorialRollDetailResponse>;

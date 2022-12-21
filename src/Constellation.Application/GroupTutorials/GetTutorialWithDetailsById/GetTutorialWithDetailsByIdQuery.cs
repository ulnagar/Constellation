namespace Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record GetTutorialWithDetailsByIdQuery(Guid Id) : IQuery<GroupTutorialDetailResponse>;

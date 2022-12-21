namespace Constellation.Application.GroupTutorials.GetTutorialById;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record GetTutorialByIdQuery(Guid TutorialId) : IQuery<GroupTutorialResponse>;

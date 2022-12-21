namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllTutorialsQuery : IQuery<List<GroupTutorialSummaryResponse>>;

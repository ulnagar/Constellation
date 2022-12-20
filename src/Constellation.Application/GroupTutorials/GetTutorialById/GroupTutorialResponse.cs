using System;

namespace Constellation.Application.GroupTutorials.GetTutorialById;

public sealed record GroupTutorialResponse(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate);

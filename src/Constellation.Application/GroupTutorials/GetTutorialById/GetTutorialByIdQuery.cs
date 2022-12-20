using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Application.GroupTutorials.GetTutorialById;

public sealed record GetTutorialByIdQuery(Guid TutorialId) : IQuery<GroupTutorialResponse>;

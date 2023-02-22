using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Application.Casuals.GetCasualById;
public sealed record GetCasualByIdQuery(
    Guid CasualId)
    : IQuery<CasualResponse>;

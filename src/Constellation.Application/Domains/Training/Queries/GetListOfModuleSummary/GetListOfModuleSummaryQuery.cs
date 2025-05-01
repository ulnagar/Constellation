namespace Constellation.Application.Domains.Training.Queries.GetListOfModuleSummary;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetListOfModuleSummaryQuery()
    : IQuery<List<ModuleSummaryDto>>;

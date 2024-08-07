﻿namespace Constellation.Application.Training.GetListOfModuleSummary;

using Constellation.Application.Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetListOfModuleSummaryQuery()
    : IQuery<List<ModuleSummaryDto>>;

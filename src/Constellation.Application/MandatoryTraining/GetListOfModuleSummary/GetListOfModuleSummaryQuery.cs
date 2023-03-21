﻿namespace Constellation.Application.MandatoryTraining.GetListOfModuleSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using System.Collections.Generic;

public sealed record GetListOfModuleSummaryQuery()
    : IQuery<List<ModuleSummaryDto>>;

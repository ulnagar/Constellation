namespace Constellation.Application.Training.GetTrainingModulesAsDictionary;

using Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetTrainingModulesAsDictionaryQuery()
    : IQuery<Dictionary<Guid, string>>;
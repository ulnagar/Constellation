﻿namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetCompletionRecordEditContextQuery(
    TrainingCompletionId Id)
    : IQuery<CompletionRecordEditContextDto>;

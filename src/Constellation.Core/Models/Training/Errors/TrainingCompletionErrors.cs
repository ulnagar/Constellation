namespace Constellation.Core.Models.Training.Errors;

using Identifiers;
using Shared;
using System;

public static class TrainingCompletionErrors
{
    public static readonly Error AlreadyExists = new(
        "MandatoryTraining.Completion.AlreadyExists",
        "A completion record with these details already exists");

    public static readonly Func<TrainingCompletionId, Error> NotFound = id => new(
        "MandatoryTraining.Completion.NotFound",
        $"A training completion record with the Id {id.Value} could not be found");
}
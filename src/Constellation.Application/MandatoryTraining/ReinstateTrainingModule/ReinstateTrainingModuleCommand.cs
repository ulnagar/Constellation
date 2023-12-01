namespace Constellation.Application.MandatoryTraining.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record ReinstateTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;

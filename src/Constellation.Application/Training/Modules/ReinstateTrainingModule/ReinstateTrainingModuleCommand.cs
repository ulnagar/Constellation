namespace Constellation.Application.Training.Modules.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record ReinstateTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;

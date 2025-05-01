namespace Constellation.Application.Domains.Training.Commands.ReinstateTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record ReinstateTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;

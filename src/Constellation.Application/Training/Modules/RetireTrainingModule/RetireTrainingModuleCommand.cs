namespace Constellation.Application.Training.Modules.RetireTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RetireTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;

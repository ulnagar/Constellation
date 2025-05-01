namespace Constellation.Application.Domains.Training.Commands.RetireTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RetireTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;

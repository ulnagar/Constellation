namespace Constellation.Application.MandatoryTraining.RetireTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RetireTrainingModuleCommand(
    TrainingModuleId Id) 
    : ICommand;

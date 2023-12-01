namespace Constellation.Application.MandatoryTraining.RetireTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record RetireTrainingModuleCommand(
    TrainingModuleId Id) 
    : ICommand;

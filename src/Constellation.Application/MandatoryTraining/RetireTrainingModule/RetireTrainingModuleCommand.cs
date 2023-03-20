namespace Constellation.Application.MandatoryTraining.RetireTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record RetireTrainingModuleCommand(
    TrainingModuleId Id) 
    : ICommand;

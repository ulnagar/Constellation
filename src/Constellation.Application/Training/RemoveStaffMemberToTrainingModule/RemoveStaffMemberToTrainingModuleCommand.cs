namespace Constellation.Application.Training.RemoveStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RemoveStaffMemberToTrainingModuleCommand(
    TrainingModuleId ModuleId,
    string StaffId)
    : ICommand;
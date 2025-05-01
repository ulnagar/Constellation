namespace Constellation.Application.Domains.Training.Commands.RemoveStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RemoveStaffMemberToTrainingModuleCommand(
    TrainingModuleId ModuleId,
    string StaffId)
    : ICommand;
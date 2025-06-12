namespace Constellation.Application.Domains.Training.Commands.RemoveStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;

public sealed record RemoveStaffMemberToTrainingModuleCommand(
    TrainingModuleId ModuleId,
    StaffId StaffId)
    : ICommand;
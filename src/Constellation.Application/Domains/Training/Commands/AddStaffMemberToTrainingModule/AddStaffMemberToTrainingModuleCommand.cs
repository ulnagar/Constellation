namespace Constellation.Application.Domains.Training.Commands.AddStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using System.Collections.Generic;

public sealed record AddStaffMemberToTrainingModuleCommand(
    TrainingModuleId ModuleId,
    List<string> StaffMemberIds)
    : ICommand;
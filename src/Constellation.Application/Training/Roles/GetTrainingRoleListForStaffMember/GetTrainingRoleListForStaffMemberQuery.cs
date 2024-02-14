namespace Constellation.Application.Training.Roles.GetTrainingRoleListForStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Training.Models;
using System.Collections.Generic;

public sealed record GetTrainingRoleListForStaffMemberQuery(
    string StaffId)
    : IQuery<List<TrainingRoleResponse>>;
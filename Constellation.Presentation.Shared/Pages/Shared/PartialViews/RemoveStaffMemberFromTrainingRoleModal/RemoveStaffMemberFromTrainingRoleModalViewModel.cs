namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.RemoveStaffMemberFromTrainingRoleModal;

using Constellation.Core.Models.Training.Identifiers;

public sealed record RemoveStaffMemberFromTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    string StaffId,
    string StaffName,
    string RoleName);
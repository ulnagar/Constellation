namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveStaffMemberFromTrainingRoleModal;

using Core.Models.Training.Identifiers;

internal sealed record RemoveStaffMemberFromTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    string StaffId,
    string StaffName,
    string RoleName);
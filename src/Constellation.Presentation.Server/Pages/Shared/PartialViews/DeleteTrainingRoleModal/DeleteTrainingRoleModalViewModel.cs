namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.DeleteTrainingRoleModal;

using Core.Models.Training.Identifiers;

internal sealed record DeleteTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    string RoleName);

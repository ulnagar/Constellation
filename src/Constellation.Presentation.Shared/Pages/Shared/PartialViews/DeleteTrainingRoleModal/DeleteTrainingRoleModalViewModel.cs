namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.DeleteTrainingRoleModal;

using Constellation.Core.Models.Training.Identifiers;

public sealed record DeleteTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    string RoleName);

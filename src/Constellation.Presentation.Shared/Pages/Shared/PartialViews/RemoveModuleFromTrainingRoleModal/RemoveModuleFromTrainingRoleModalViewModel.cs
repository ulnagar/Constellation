namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.RemoveModuleFromTrainingRoleModal;

using Constellation.Core.Models.Training.Identifiers;

public sealed record RemoveModuleFromTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    TrainingModuleId ModuleId,
    string RoleName,
    string ModuleName);
namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.RemoveModuleFromTrainingRoleModal;

using Constellation.Core.Models.Training.Identifiers;

internal sealed record RemoveModuleFromTrainingRoleModalViewModel(
    TrainingRoleId RoleId,
    TrainingModuleId ModuleId,
    string RoleName,
    string ModuleName);
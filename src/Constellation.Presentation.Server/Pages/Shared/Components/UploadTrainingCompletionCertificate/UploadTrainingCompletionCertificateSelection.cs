namespace Constellation.Presentation.Server.Pages.Shared.Components.UploadTrainingCompletionCertificate;

using Core.Models.Training.Identifiers;

internal sealed record UploadTrainingCompletionCertificateSelection(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId,
    DateOnly CompletionDate);
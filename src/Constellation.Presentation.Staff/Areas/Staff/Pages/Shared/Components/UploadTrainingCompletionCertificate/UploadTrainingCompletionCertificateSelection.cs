namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UploadTrainingCompletionCertificate;

using Constellation.Core.Models.Training.Identifiers;

public sealed record UploadTrainingCompletionCertificateSelection(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId,
    DateOnly CompletionDate);

public enum CompletionPageMode
{
    Full = 0,
    SoloStaff = 1,
    SoloModule = 2,
    CertUpload = 3
}
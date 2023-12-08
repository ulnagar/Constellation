namespace Constellation.Presentation.Server.ViewComponents;

using Core.Models.Training.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.UploadTrainingCompletionCertificate;

public class UploadTrainingCompletionCertificateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Guid moduleId, Guid completionId, DateOnly completionDate)
    {
        TrainingModuleId module = TrainingModuleId.FromValue(moduleId);
        TrainingCompletionId completion = TrainingCompletionId.FromValue(completionId);

        UploadTrainingCompletionCertificateSelection viewModel = new(
            module,
            completion,
            completionDate);

        return View(viewModel);
    }
}
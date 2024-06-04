namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Core.Models.Training.Identifiers;
using Constellation.Presentation.Shared.Pages.Shared.Components.UploadTrainingCompletionCertificate;
using Microsoft.AspNetCore.Mvc;

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
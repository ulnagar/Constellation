namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.CreateMessageDraftFromAssessment;

using Constellation.Core.Models.Assessments.Identifiers;
using Microsoft.AspNetCore.Mvc;

public class CreateMessageDraftFromAssessmentViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        CreateMessageDraftFromAssessmentSelection viewModel = new();

        return View(viewModel);
    }
}

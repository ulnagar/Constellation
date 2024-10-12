namespace Constellation.Presentation.Students.Areas.Students.Pages.Shared.Components.StudentNav;

using Microsoft.AspNetCore.Mvc;

public class StudentNavViewComponent : ViewComponent
{
    public StudentNavViewComponent() { }

    public IViewComponentResult Invoke(string activePage) => View("StudentNav", activePage);
}
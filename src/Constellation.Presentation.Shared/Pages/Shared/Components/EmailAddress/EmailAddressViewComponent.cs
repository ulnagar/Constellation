namespace Constellation.Presentation.Shared.Pages.Shared.Components.EmailAddress;

using Microsoft.AspNetCore.Mvc;

public sealed class EmailAddressViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string email) => View("EmailAddress", email);
}

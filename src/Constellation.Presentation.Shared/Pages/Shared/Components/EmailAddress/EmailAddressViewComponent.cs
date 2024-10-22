namespace Constellation.Presentation.Shared.Pages.Shared.Components.EmailAddress;

using Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;

public sealed class EmailAddressViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(EmailAddress email) => View("EmailAddress", email);
}

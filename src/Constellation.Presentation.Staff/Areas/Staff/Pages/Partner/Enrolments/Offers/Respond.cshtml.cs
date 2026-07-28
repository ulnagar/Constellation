namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Offers;

using Application.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_Edit_Value)]
public class RespondModel : BasePageModel
{
    public void OnGet()
    {
    }
}
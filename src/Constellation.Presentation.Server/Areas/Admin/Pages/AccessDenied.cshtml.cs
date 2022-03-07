using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;

namespace Constellation.Presentation.Server.Areas.Admin.Pages
{
    [AllowAnonymous]
    public class AccessDeniedModel : BasePageModel
    {
        public AccessDeniedModel()
            : base()
        {
        }

        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }
    }
}

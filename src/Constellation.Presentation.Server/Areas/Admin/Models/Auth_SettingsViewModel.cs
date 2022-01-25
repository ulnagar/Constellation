using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Auth_SettingsViewModel : BaseViewModel
    {
        public ICollection<AppUser> Users { get; set; }
        public ICollection<AppAccessToken> Tokens { get; set; }
        public ICollection<AppRole> Roles { get; set; }

        public Auth_SettingsViewModel()
        {
            Users = new List<AppUser>();
            Roles = new List<AppRole>();
        }
    }
}
using Constellation.Presentation.Server.BaseModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Auth_CreateTokenViewModel : BaseViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Expiry { get; set; }

        public string RedirectTo { get; set; }

        public Auth_CreateTokenViewModel()
        {
            Expiry = DateTime.Today.AddDays(1);
        }
    }
}
using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Casual_UpdateViewModel : BaseViewModel
    {
        public Casual_UpdateViewModel()
        {
            Casual = new CasualDto();
        }

        public CasualDto Casual { get; set; }
        public bool IsNew { get; set; }
        public SelectList SchoolList { get; set; }
    }
}
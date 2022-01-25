using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.ShortTerm.Models
{
    public class Covers_CreateViewModel : BaseViewModel
    {
        public Covers_CreateViewModel()
        {
            Cover = new CoverDto();
        }

        public CoverDto Cover { get; set; }
        public SelectList UserList { get; set; }
        public SelectList ClassList { get; set; }
        public SelectList TeacherList { get; set; }
        public MultiSelectList MultiClassList { get; set; }
    }
}
using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_UpdateViewModel : BaseViewModel
    {
        //Student Object
        public StudentDto Student { get; set; }

        //View properties
        public SelectList SchoolList { get; set; }
        public SelectList GenderList { get; set; }
        public bool IsNew { get; set; }
    }
}
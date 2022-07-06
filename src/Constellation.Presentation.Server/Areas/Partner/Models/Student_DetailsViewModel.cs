using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_DetailsViewModel : BaseViewModel
    {
        public StudentCompleteDetailsDto Student { get; set; }

        // View Properties
        public int MinPerFn { get; set; }
        public SelectList OfferingList { get; set; }


    }
}
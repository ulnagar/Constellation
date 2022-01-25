using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Student_InterviewDetails_ViewModel : BaseViewModel
    {
        public Student_InterviewDetails_ViewModel()
        {
            Filter = new InterviewExportSelectionDto();
        }

        public InterviewExportSelectionDto Filter { get; set; }
        public SelectList AllClasses { get; set; }
    }
}
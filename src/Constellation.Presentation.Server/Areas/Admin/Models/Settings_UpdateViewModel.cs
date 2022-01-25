using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Settings_UpdateViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }

        public string AdobeConnectDefaultFolder { get; set; }

        public string SentralContactPreference { get; set; }

        [EmailAddress]
        public string LessonsCoordinatorEmail { get; set; }
        public string LessonsCoordinatorName { get; set; }
        public string LessonsCoordinatorTitle { get; set; }
        [EmailAddress]
        public string LessonsHeadTeacherEmail { get; set; }

        public SelectList SentralContactPreferenceList { get; set; }
    }
}
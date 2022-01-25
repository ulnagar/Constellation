using Constellation.Presentation.Server.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class AdobeConnectRoomDto
    {
        [Display(Name = DisplayNameDefaults.AdobeConnectId)]
        public string ScoId { get; set; }
        public string Name { get; set; }
        [Display(Name = DisplayNameDefaults.UrlPath)]
        public string UrlPath { get; set; }
        [Display(Name = DisplayNameDefaults.DateStart)]
        public string DateStart { get; set; }
        [Display(Name = DisplayNameDefaults.DateEnd)]
        public string DateEnd { get; set; }
        [Display(Name = DisplayNameDefaults.DetectParentFolder)]
        public bool DetectParentFolder { get; set; }
        [Display(Name = DisplayNameDefaults.ParentFolder)]
        public string ParentFolder { get; set; }
        public string Year { get; set; }
        public string Grade { get; set; }
        [Display(Name = DisplayNameDefaults.UseTemplate)]
        public bool UseTemplate { get; set; }
        [Display(Name = DisplayNameDefaults.RoomFaculty)]
        public string RoomFaculty { get; set; }
        public bool Protected { get; set; }
        public string Action { get; set; }
    }
}

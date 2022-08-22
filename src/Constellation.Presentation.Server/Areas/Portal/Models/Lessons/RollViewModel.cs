using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class RollViewModel : BaseViewModel
    {
        public LessonRollDto Roll { get; set; }
    }
}
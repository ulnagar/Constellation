using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminTeacherAuditViewModel : BaseViewModel
    {
        public AdminTeacherAuditViewModel()
        {
            AuditResult = new UserAuditDto();
        }

        public UserAuditDto AuditResult { get; set; }
    }
}
using Constellation.Application.DTOs;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminTeacherAuditViewModel
    {
        public AdminTeacherAuditViewModel()
        {
            AuditResult = new UserAuditDto();
        }

        public UserAuditDto AuditResult { get; set; }
    }
}
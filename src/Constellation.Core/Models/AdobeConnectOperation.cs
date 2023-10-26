using Constellation.Core.Enums;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Students;
using System;

namespace Constellation.Core.Models
{
    public abstract class AdobeConnectOperation
    {
        public int Id { get; set; }
        public string GroupSco { get; set; }
        public string ScoId { get; set; }
        public AdobeConnectRoom Room { get; set; }
        public string PrincipalId { get; set; }
        public AdobeConnectOperationAction Action { get; set; }
        public DateTime DateScheduled { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CoverId { get; set; }

        public void Delete()
        {
            IsDeleted = true;
        }
    }

    public class StudentAdobeConnectOperation : AdobeConnectOperation
    {
        public string StudentId { get; set; }
        public Student Student { get; set; }
    }

    public class CasualAdobeConnectOperation : AdobeConnectOperation
    {
        public Guid CasualId { get; set; }
    }

    public class TeacherAdobeConnectOperation : AdobeConnectOperation
    {
        public string StaffId { get; set; }
        public Staff Teacher { get; set; }
    }

    public class TeacherAdobeConnectGroupOperation : AdobeConnectOperation
    {
        public string TeacherId { get; set; }
        public Staff Teacher { get; set; }
        public string GroupName { get; set; }
    }
}
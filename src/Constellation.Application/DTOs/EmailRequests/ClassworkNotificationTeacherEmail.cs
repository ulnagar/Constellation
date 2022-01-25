using System;
using System.Collections.Generic;

namespace Constellation.Application.DTOs.EmailRequests
{
    public class ClassworkNotificationTeacherEmail
    {
        public ClassworkNotificationTeacherEmail()
        {
            Students = new List<string>();
            Teachers = new List<Teacher>();
        }

        public string OfferingName { get; set; }
        public DateTime AbsenceDate { get; set; }
        public ICollection<string> Students { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsCovered { get; set; }
        public ICollection<Teacher> Teachers { get; set; }

        public class Teacher
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}

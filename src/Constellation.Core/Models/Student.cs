using Constellation.Core.Enums;
using Constellation.Core.Models.Families;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Core.Models
{
    public class Student
    {
        public Student()
        {
            IsDeleted = false;
            DateEntered = DateTime.Now;

            Enrolments = new List<Enrolment>();
            Devices = new List<DeviceAllocation>();
            AdobeConnectOperations = new List<StudentAdobeConnectOperation>();
            MSTeamOperations = new List<StudentMSTeamOperation>();

            Absences = new List<Absence>();
            PartialAbsences = new List<StudentPartialAbsence>();
            WholeAbsences = new List<StudentWholeAbsence>();

            LessonsAttended = new List<LessonRoll.LessonRollStudentAttendance>();

            //Awards = new List<StudentAward>();
        }

        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PortalUsername { get; set; }
        public string AdobeConnectPrincipalId { get; set; }
        public string SentralStudentId { get; set; }
        public Grade CurrentGrade { get; set; }
        public Grade EnrolledGrade { get; set; }
        public string Gender { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateEntered { get; set; }
        public string SchoolCode { get; set; }
        public School School { get; set; }
        public bool IncludeInAbsenceNotifications { get; set; }
        public DateTime? AbsenceNotificationStartDate { get; set; }
        public string DisplayName => FirstName.Trim() + " " + LastName.Trim();
        public string EmailAddress => PortalUsername + "@education.nsw.gov.au";
        public byte[] Photo { get; set; }

        public List<StudentFamilyMembership> FamilyMemberships { get; set; } = new();
        public ICollection<Enrolment> Enrolments { get; set; }
        public ICollection<DeviceAllocation> Devices { get; set; }
        public ICollection<StudentAdobeConnectOperation> AdobeConnectOperations { get; set; }
        public ICollection<StudentMSTeamOperation> MSTeamOperations { get; set; }
        public ICollection<Absence> Absences { get; set; }
        public ICollection<StudentPartialAbsence> PartialAbsences { get; set; }
        public ICollection<StudentWholeAbsence> WholeAbsences { get; set; }
        public ICollection<LessonRoll.LessonRollStudentAttendance> LessonsAttended { get; set; }
        public ICollection<StudentAward> Awards { get; set; }

        public Task EnableAbsenceNotifications(DateTime startDate)
        {
            IncludeInAbsenceNotifications = true;
            AbsenceNotificationStartDate = startDate;
            return Task.CompletedTask;
        }
    }
}
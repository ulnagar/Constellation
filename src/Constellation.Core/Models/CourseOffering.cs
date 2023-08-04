using System;
using System.Collections.Generic;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.SciencePracs;

namespace Constellation.Core.Models
{
    public class CourseOffering
    {
        public CourseOffering()
        {
            Enrolments = new List<Enrolment>();
            Sessions = new List<OfferingSession>();
            Resources = new List<OfferingResource>();
            Absences = new List<Absence>();
        }

        public CourseOffering(int courseId, DateTime startDate, DateTime endDate)
        {
            Enrolments = new List<Enrolment>();
            Sessions = new List<OfferingSession>();
            Resources = new List<OfferingResource>();
            Absences = new List<Absence>();

            CourseId = courseId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public int Id { get;  set; }
        public string Name { get;  set; }
        public int CourseId { get;  set; }
        public Course Course { get;  set; }
        public DateTime StartDate { get;  set; }
        public DateTime EndDate { get;  set; }
        public ICollection<Enrolment> Enrolments { get;  set; }
        public ICollection<OfferingSession> Sessions { get;  set; }
        public ICollection<OfferingResource> Resources { get;  set; }
        public ICollection<SciencePracLesson> Lessons { get; set; }
        public ICollection<Absence> Absences { get; set; }
    }
}
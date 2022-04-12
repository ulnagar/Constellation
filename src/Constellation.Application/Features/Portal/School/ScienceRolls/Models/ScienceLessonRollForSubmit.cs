using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.Features.Portal.School.ScienceRolls.Models
{
    public class ScienceLessonRollForSubmit : IMapFrom<LessonRoll>
    {
        public Guid Id { get; set; }
        
        public Guid LessonId { get; set; }
        
        [Display(Name = DisplayNameDefaults.LessonName)]
        public string LessonName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = DisplayNameDefaults.DueDate)]
        public DateTime LessonDueDate { get; set; }

        [Display(Name = DisplayNameDefaults.SubmittedDate)]
        public DateTime? SubmittedDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = DisplayNameDefaults.SubmittedDate)]
        public DateTime LessonDate { get; set; } = DateTime.Today;

        public string Comment { get; set; }

        public ICollection<StudentAttendance> Attendance { get; set; } = new List<StudentAttendance>();
        
        [Display(Name = DisplayNameDefaults.TeacherName)]
        public string TeacherName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<LessonRoll, ScienceLessonRollForSubmit>()
                .ForMember(dest => dest.TeacherName, opt => opt.Ignore());
        }

        public class StudentAttendance : IMapFrom<LessonRoll.LessonRollStudentAttendance>
        {
            public Guid Id { get; set; }

            public string StudentFirstName { get; set; }
            public string StudentLastName { get; set; }
            public string StudentName => $"{StudentFirstName} {StudentLastName}";
            
            public bool Present { get; set; }
        }
    }
}

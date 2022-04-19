using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class StudentDto
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";

        // Not required but highly desirable
        public string PortalUsername { get; set; }
        public string AdobeConnectPrincipalId { get; set; }
        public string SentralStudentId { get; set; }

        [Required]
        public Grade EnrolledGrade { get; set; }
        public Grade CurrentGrade { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string SchoolCode { get; set; }

        public string PhotoSrc { get; set; }

        public static StudentDto ConvertFromStudent(Student student)
        {
            var viewModel = new StudentDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PortalUsername = student.PortalUsername,
                AdobeConnectPrincipalId = student.AdobeConnectPrincipalId,
                SentralStudentId = student.SentralStudentId,
                EnrolledGrade = student.EnrolledGrade,
                CurrentGrade = student.CurrentGrade,
                Gender = student.Gender,
                SchoolCode = student.SchoolCode,
                PhotoSrc = $"data:image/jpg;base64,{Convert.ToBase64String(student.Photo)}"
            };

            return viewModel;
        }
    }
}

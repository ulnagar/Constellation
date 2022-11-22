using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class School_DetailsViewModel : BaseViewModel
    {
        public School_DetailsViewModel()
        {
            Contacts = new List<ContactDto>();
            Students = new List<StudentDto>();
            Staff = new List<StaffDto>();
        }

        public SchoolDto School { get; set; }
        public ICollection<ContactDto> Contacts { get; set; }
        public ICollection<StudentDto> Students { get; set; }
        public ICollection<StaffDto> Staff { get; set; }
        public Contacts_AssignmentViewModel RoleAssignmentDto { get; set; }

        public class SchoolDto
        {
            [Display(Name=DisplayNameDefaults.SchoolCode)]
            public string SchoolCode { get; set; }
            public string Name { get; set; }
            [Display(Name=DisplayNameDefaults.HasStudents)]
            public bool HasStudents { get; set; }
            public string Address { get; set; }
            [Display(Name=DisplayNameDefaults.HasStaff)]
            public bool HasStaff { get; set; }
            public string Town { get; set; }
            public string Division { get; set; }
            public string State { get; set; }
            [Display(Name=DisplayNameDefaults.HeatSchool)]
            public bool HeatSchool { get; set; }
            [Display(Name=DisplayNameDefaults.PostCode)]
            public string PostCode { get; set; }
            public string Electorate { get; set; }
            [Display(Name=DisplayNameDefaults.PhoneNumber)]
            public string PhoneNumber { get; set; }
            [Display(Name=DisplayNameDefaults.PrincipalNetwork)]
            public string PrincipalNetwork { get; set; }
            [Display(Name=DisplayNameDefaults.FaxNumber)]
            public string FaxNumber { get; set; }
            [Display(Name=DisplayNameDefaults.TimetableApplication)]
            public string TimetableApplication { get; set; }
            [Display(Name=DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name=DisplayNameDefaults.RollCallGroup)]
            public string RollCallGroup { get; set; }

            public static SchoolDto ConvertFromSchool(School school)
            {
                var viewModel = new SchoolDto
                {
                    SchoolCode = school.Code,
                    Name = school.Name,
                    Address = school.Address,
                    Town = school.Town,
                    Division = school.Division,
                    State = school.State,
                    HeatSchool = school.HeatSchool,
                    PostCode = school.PostCode,
                    Electorate = school.Electorate,
                    PhoneNumber = school.PhoneNumber,
                    PrincipalNetwork = school.PrincipalNetwork,
                    FaxNumber = school.FaxNumber,
                    TimetableApplication = school.TimetableApplication,
                    EmailAddress = school.EmailAddress,
                    RollCallGroup = school.RollCallGroup,
                    HasStudents = school.Students.Any(student => !student.IsDeleted),
                    HasStaff = school.Staff.Any(staff => !staff.IsDeleted)
                };

                return viewModel;
            }
        }

        public class ContactDto
        {
            public int RoleId { get; set; }
            public string Name { get; set; }
            [Display(Name=DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name=DisplayNameDefaults.PhoneNumber)]
            public string PhoneNumber { get; set; }
            public string Role { get; set; }

            public static ContactDto ConvertFromAssignment(SchoolContactRole role)
            {
                var viewModel = new ContactDto
                {
                    RoleId = role.Id,
                    Name = role.SchoolContact.DisplayName,
                    EmailAddress = role.SchoolContact.EmailAddress,
                    PhoneNumber = role.SchoolContact.PhoneNumber,
                    Role = role.Role
                };

                return viewModel;
            }
        }

        public class StudentDto
        {
            public StudentDto()
            {
                Enrolments = new List<string>();
            }

            [Display(Name=DisplayNameDefaults.StudentId)]
            public string StudentId { get; set; }
            public string Gender { get; set; }
            public string Name { get; set; }
            public Grade Grade { get; set; }
            public ICollection<string> Enrolments { get; set; }

            public static StudentDto ConvertFromStudent(Student student)
            {
                var viewModel = new StudentDto
                {
                    StudentId = student.StudentId,
                    Gender = student.Gender,
                    Name = student.DisplayName,
                    Grade = student.CurrentGrade,
                    Enrolments = student.Enrolments.Where(enrol => !enrol.IsDeleted).Select(enrol => enrol.Offering.Name).ToList()
                };

                return viewModel;
            }
        }

        public class StaffDto
        {
            public StaffDto()
            {
                Courses = new List<string>();
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> Faculty { get; set; } = new();
            public ICollection<string> Courses { get; set; }

            public static StaffDto ConvertFromStaff(Staff staff)
            {
                var viewModel = new StaffDto
                {
                    Id = staff.StaffId,
                    Name = staff.DisplayName,
                    Faculty = staff.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty.Name).ToList(),
                    Courses = staff.CourseSessions.Where(session => !session.IsDeleted).Select(session => session.Offering.Name).Distinct().ToList()
                };

                return viewModel;
            }
        }
    }
}
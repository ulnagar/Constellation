using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence.TrackIt;
using Constellation.Infrastructure.Persistence.TrackIt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class TrackItSyncJob : ITrackItSyncJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TrackItContext _tiContext;
        private readonly ILogger<ITrackItSyncJob> _logger;
        private static int _newCustomerSequence;
        private static int _newLocationSequence;

        private Guid JobId { get; set; }

        public TrackItSyncJob(IUnitOfWork unitOfWork, TrackItContext tiContext, ILogger<ITrackItSyncJob> logger)
        {
            _unitOfWork = unitOfWork;
            _tiContext = tiContext;
            _logger = logger;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            JobId = jobId;

            var acosStudents = await _unitOfWork.Students.ForTrackItSync();
            var acosStaff = await _unitOfWork.Staff.AllActiveAsync();
            var acosSchools = await _unitOfWork.Schools.ForTrackItSync();

            var tiCustomers = await _tiContext.Customers.ToListAsync();
            var tiLocations = await _tiContext.Locations.ToListAsync();

            foreach (var acosSchool in acosSchools)
            {
                _logger.LogInformation("{id}: School: Name {school} - Code {code}", jobId, acosSchool.Name, acosSchool.Code);
                var tiLocation = tiLocations.FirstOrDefault(c => c.Note == acosSchool.Code);
                if (tiLocation != null)
                {
                    CheckExistingLocationDetail(tiLocation, acosSchool);
                }
                else
                {
                    var location = CreateLocationFromSchool(acosSchool);
                    _tiContext.Locations.Add(location);
                }
            }

            SetNextLocationSequence();
            await _tiContext.SaveChangesAsync();

            foreach (var customer in tiCustomers)
            {
                if (customer.Emailid != null)
                    customer.Emailid = ConvertEmailToEmailId(ConvertEmailIdToEmail(customer.Emailid));
            }

            _tiContext.SaveChanges();

            foreach (var acosStudent in acosStudents)
            {
                _logger.LogInformation("{id}: Student: Name {student} - Email {emailAddress}", jobId, acosStudent.DisplayName, acosStudent.EmailAddress);
                var customerEmailId = ConvertEmailToEmailId(acosStudent.EmailAddress);
                var tiCustomer = tiCustomers.FirstOrDefault(c => c.Client == CreateClientFromPortalUsername(acosStudent.PortalUsername) || c.Emailid == customerEmailId);
                if (tiCustomer != null)
                {
                    CheckExistingCustomerDetail(tiCustomer, acosStudent);
                }
                else
                {
                    var customer = CreateCustomerFromStudent(acosStudent);
                    _tiContext.Customers.Add(customer);
                }
            }

            SetNextCustomerSequence();
            await _tiContext.SaveChangesAsync();

            foreach (var acosStaffMember in acosStaff)
            {
                _logger.LogInformation("{id}: Teacher: Name {teacher} - Email {emailAddress}", jobId, acosStaffMember.DisplayName, acosStaffMember.EmailAddress);
                var customerEmailId = ConvertEmailToEmailId(acosStaffMember.EmailAddress);
                var tiCustomer = tiCustomers.FirstOrDefault(c => c.Client == CreateClientFromPortalUsername(acosStaffMember.PortalUsername) || c.Emailid == customerEmailId);
                if (tiCustomer != null)
                {
                    CheckExistingCustomerDetail(tiCustomer, acosStaffMember);
                }
                else
                {
                    var customer = CreateCustomerFromStaff(acosStaffMember);
                    _tiContext.Customers.Add(customer);
                }
            }

            SetNextCustomerSequence();
            await _tiContext.SaveChangesAsync();
        }

        private void CheckExistingLocationDetail(Location location, School school)
        {
            if (location.Name != ConvertSchoolNameToLocationName(school.Name))
            {
                location.Name = school.Name;
                _logger.LogInformation("{id}: School: Name {school} - Code {code}: Name updated to {newName}", JobId, school.Name, school.Code, location.Name);
            }

            if (location.Address != school.Address)
            {
                location.Address = school.Address;
                _logger.LogInformation("{id}: School: Name {school} - Code {code}: Address updated to {newAddress}", JobId, school.Name, school.Code, location.Address);
            }

            if (location.City != school.Town)
            {
                location.City = school.Town;
                _logger.LogInformation("{id}: School: Name {school} - Code {code}: City updated to {newCity}", JobId, school.Name, school.Code, location.City);
            }

            if (location.Zip != school.PostCode)
            {
                location.Zip = school.PostCode;
                _logger.LogInformation("{id}: School: Name {school} - Code {code}: PostCode updated to {newPostCode}", JobId, school.Name, school.Code, location.Zip);
            }

            if (location.Phone != school.PhoneNumber)
            {
                location.Phone = school.PhoneNumber;
                _logger.LogInformation("{id}: School: Name {school} - Code {code}: PhoneNumber updated to {newPhone}", JobId, school.Name, school.Code, location.Phone);
            }

            var acc = school.StaffAssignments.FirstOrDefault(s => s.Role == SchoolContactRole.Coordinator)?.SchoolContact;
            if (acc != null)
            {
                location.MainContact = acc.DisplayName;
                location.Maincontctphone = acc.PhoneNumber;

                _logger.LogInformation("{id}: School: Name {school} - Code {code}: ACC updated to {newACC}", JobId, school.Name, school.Code, acc.DisplayName);
            }
            else
            {
                location.MainContact = null;
                location.Maincontctphone = null;
            }

            location.Updated();
        }

        private Location CreateLocationFromSchool(School school)
        {
            var location = new Location()
            {
                Note = school.Code,
                Name = ConvertSchoolNameToLocationName(school.Name),
                Address = school.Address,
                City = school.Town,
                Zip = school.PostCode,
                Phone = school.PhoneNumber
            };

            _logger.LogInformation("{id}: School: Name {school} - Code {code}: Created new record", JobId, school.Name, school.Code);


            var acc = school.StaffAssignments.FirstOrDefault(s => s.Role == SchoolContactRole.Coordinator)?.SchoolContact;
            if (acc != null)
            {
                location.MainContact = acc.DisplayName;
                location.Maincontctphone = acc.PhoneNumber;

                _logger.LogInformation("{id}: School: Name {school} - Code {code}: ACC updated to {newACC}", JobId, school.Name, school.Code, acc.DisplayName);
            }

            location.Sequence = GetNextLocationSequence();

            return location;
        }

        private static string CreateClientFromPortalUsername(string portalUsername)
        {
            if (portalUsername.Length > 15)
            {
                return portalUsername.Substring(0, 15);
            }

            return portalUsername;
        }

        private static string ConvertSchoolNameToLocationName(string schoolName)
        {
            var locationName = schoolName;
            if (locationName.Length > 50 && locationName.Contains("Secondary College"))
            {
                locationName = locationName.Replace("Secondary College", "SC");
                return locationName;
            }

            if (locationName.Length > 50 && locationName.Contains("Environmental Education Centre"))
            {
                locationName = locationName.Replace("Environmental Education Centre", "EEC");
                return locationName;
            }

            if (locationName.Length > 50 && locationName.Contains("Creative and Performing Arts"))
            {
                locationName = locationName.Replace("Creative and Performing Arts", "CAPA");
                return locationName;
            }

            if (locationName.Length > 50 && locationName.Contains(","))
            {
                locationName = locationName.Substring(locationName.IndexOf(",") + 2);
                return locationName;
            }

            return locationName;
        }

        private static string ConvertEmailIdToEmail(string emailId)
        {
            return emailId.Substring(emailId.LastIndexOf("}") + 1).ToLower();
        }

        private static string ConvertEmailToEmailId(string email)
        {
            return $"SMTP:{{{email.ToLower()}}}{email.ToLower()}";
        }

        private void CheckExistingCustomerDetail(Customer customer, Student student)
        {
            if (customer.Client != CreateClientFromPortalUsername(student.PortalUsername).ToUpper())
                customer.Client = CreateClientFromPortalUsername(student.PortalUsername).ToUpper();

            customer.Emailid = ConvertEmailToEmailId(student.EmailAddress);

            if (customer.Fname != student.FirstName)
            {
                customer.Fname = student.FirstName;

                _logger.LogInformation("{id}: Student: Name {student} - Email {emailAddress}: FirstName updated to {newName}", JobId, student.DisplayName, student.EmailAddress, student.FirstName);
            }

            if (customer.Name != student.LastName)
            {
                customer.Name = student.LastName;

                _logger.LogInformation("{id}: Student: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, student.DisplayName, student.EmailAddress, student.LastName);
            }

            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name == "Students");
            customer.Dept = department?.Sequence;

            var location = _tiContext.Locations.FirstOrDefault(c => c.Note == student.SchoolCode);
            customer.Location = location?.Sequence;

            customer.Updated();
        }

        private void CheckExistingCustomerDetail(Customer customer, Staff staff)
        {
            if (customer.Client != CreateClientFromPortalUsername(staff.PortalUsername).ToUpper())
                customer.Client = CreateClientFromPortalUsername(staff.PortalUsername).ToUpper();

            customer.Emailid = ConvertEmailToEmailId(staff.EmailAddress);

            if (customer.Fname != staff.FirstName)
            {
                customer.Fname = staff.FirstName;

                _logger.LogInformation("{id}: Staff: Name {staff} - Email {emailAddress}: FirstName updated to {newName}", JobId, staff.DisplayName, staff.EmailAddress, staff.FirstName);
            }

            if (customer.Name != staff.LastName)
            {
                customer.Name = staff.LastName;

                _logger.LogInformation("{id}: Staff: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, staff.DisplayName, staff.EmailAddress, staff.LastName);
            }

            var faculty = staff.Faculty.ToString();
            if (faculty.Contains(","))
                faculty = faculty.Substring(0, faculty.IndexOf(","));
            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name.Contains(faculty));
            customer.Dept = department?.Sequence;

            var location = _tiContext.Locations.FirstOrDefault(c => c.Note == staff.SchoolCode);
            customer.Location = location?.Sequence;

            customer.Updated();
        }

        private Customer CreateCustomerFromStudent(Student student)
        {
            var customer = new Customer
            {
                Client = CreateClientFromPortalUsername(student.PortalUsername).ToUpper(),
                Emailid = ConvertEmailToEmailId(student.EmailAddress),
                Group = 2,
                Inactive = 0,
                Fname = student.FirstName,
                Name = student.LastName
            };

            _logger.LogInformation("{id}: Student: Name {student} - Email {emailAddress}: Created new record", JobId, student.DisplayName, student.EmailAddress);

            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name == "Students");
            customer.Dept = department?.Sequence;

            var location = _tiContext.Locations.ToList().FirstOrDefault(c => c.Note == student.SchoolCode);
            customer.Location = location?.Sequence;

            customer.Sequence = GetNextCustomerSequence();

            return customer;
        }

        private Customer CreateCustomerFromStaff(Staff staff)
        {
            var customer = new Customer
            {
                Client = staff.PortalUsername.ToUpper(),
                Emailid = ConvertEmailToEmailId(staff.EmailAddress),
                Group = 2,
                Inactive = 0,
                Fname = staff.FirstName,
                Name = staff.LastName
            };

            _logger.LogInformation("{id}: Staff: Name {staff} - Email {emailAddress}: Created new record", JobId, staff.DisplayName, staff.EmailAddress);

            var faculty = staff.Faculty.ToString();
            if (faculty.Contains(","))
                faculty = faculty.Substring(0, faculty.IndexOf(","));
            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name.Contains(faculty));
            customer.Dept = department?.Sequence;

            var location = _tiContext.Locations.ToList().FirstOrDefault(c => c.Note == staff.SchoolCode);
            customer.Location = location?.Sequence;

            customer.Sequence = GetNextCustomerSequence();

            return customer;
        }

        private int GetNextCustomerSequence()
        {
            if (_newCustomerSequence == 0)
            {
                var current = _tiContext.Indexes.ToList().First(c => c.Name == "_CUSTOMER_").Recnum;

                current++;

                _newCustomerSequence = current;

                return current;
            }
            else
            {
                _newCustomerSequence++;

                return _newCustomerSequence;
            }
        }

        private void SetNextCustomerSequence()
        {
            var current = _tiContext.Indexes.ToList().First(c => c.Name == "_CUSTOMER_");

            if (_newCustomerSequence > current.Recnum)
            {
                current.Recnum = _newCustomerSequence;
                current.Lastmodified = DateTime.Now;
            }
        }

        private int GetNextLocationSequence()
        {
            if (_newLocationSequence == 0)
            {
                var current = _tiContext.Indexes.ToList().First(c => c.Name == "_LOCATION_").Recnum;

                current++;

                _newLocationSequence = current;

                return current;
            }
            else
            {
                _newLocationSequence++;

                return _newLocationSequence;
            }
        }

        private void SetNextLocationSequence()
        {
            var current = _tiContext.Indexes.ToList().First(c => c.Name == "_LOCATION_");

            if (_newLocationSequence > current.Recnum)
            {
                current.Recnum = _newLocationSequence;
                current.Lastmodified = DateTime.Now;
            }
        }
    }
}

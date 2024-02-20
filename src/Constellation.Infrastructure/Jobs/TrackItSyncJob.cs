namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Persistence.TrackItContext;
using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Core.Models.Faculty;
using Core.Models.Faculty.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class TrackItSyncJob : ITrackItSyncJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly TrackItContext _tiContext;
    private readonly ILogger _logger;
    private static int _newCustomerSequence;
    private static int _newLocationSequence;

    private Guid JobId { get; set; }

    public TrackItSyncJob(
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        TrackItContext tiContext, 
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _tiContext = tiContext;
        _logger = logger.ForContext<ITrackItSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        JobId = jobId;

        List<Student> acosStudents = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);
        List<School> acosSchools = await _schoolRepository.GetAllActive(cancellationToken);
        List<Staff> acosStaff = await _staffRepository.GetAllActive(cancellationToken);

        var tiCustomers = await _tiContext.Customers.ToListAsync(cancellationToken);
        var tiLocations = await _tiContext.Locations.ToListAsync(cancellationToken);

        foreach (School acosSchool in acosSchools)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: School: Name {school} - Code {code}", jobId, acosSchool.Name, acosSchool.Code);
            Location tiLocation = tiLocations.FirstOrDefault(c => c.Note == acosSchool.Code);
            if (tiLocation is not null)
                CheckExistingLocationDetail(tiLocation, acosSchool);
            else
            { 
                Location location = CreateLocationFromSchool(acosSchool);
                _tiContext.Locations.Add(location);
            }
        }

        SetNextLocationSequence();
        await _tiContext.SaveChangesAsync(cancellationToken);

        foreach (Customer customer in tiCustomers)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (customer.Emailid is null)
                continue;

            string emailAddress = ConvertEmailIdToEmail(customer.Emailid);
            customer.Emailid = ConvertEmailToEmailId(emailAddress);
            
            if (customer.Inactive == 1)
                continue;

            if (emailAddress.Contains("@det.nsw.edu.au"))
            {
                Staff staffMember = await _staffRepository.GetAnyByEmailAddress(emailAddress, cancellationToken);

                if (staffMember is not null && !staffMember.IsDeleted)
                    continue;

                if (staffMember is not null && staffMember.DateDeleted.HasValue && staffMember.DateDeleted.Value.AddDays(7) > DateTime.Today)
                    continue;

                _logger.Information("{id}: Customer: {user} no longer active - removing", jobId, emailAddress);

                customer.Inactive = 1;
            }

            if (emailAddress.Contains("@education.nsw.gov.au"))
            {
                Student student = await _studentRepository.GetAnyByEmailAddress(ConvertEmailIdToEmail(customer.Emailid), cancellationToken);

                if (student is not null && !student.IsDeleted)
                    continue;

                if (student is not null && student.DateDeleted.HasValue && student.DateDeleted.Value.AddDays(7) > DateTime.Today)
                    continue;

                _logger.Information("{id}: Customer: {user} no longer active - removing", jobId, emailAddress);

                customer.Inactive = 1;
            }
        }

        await _tiContext.SaveChangesAsync(cancellationToken);

        foreach (var acosStudent in acosStudents)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}", jobId, acosStudent.DisplayName, acosStudent.EmailAddress);
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
        await _tiContext.SaveChangesAsync(cancellationToken);

        foreach (var acosStaffMember in acosStaff)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Teacher: Name {teacher} - Email {emailAddress}", jobId, acosStaffMember.DisplayName, acosStaffMember.EmailAddress);
            var customerEmailId = ConvertEmailToEmailId(acosStaffMember.EmailAddress);
            var tiCustomer = tiCustomers.FirstOrDefault(c => c.Client == CreateClientFromPortalUsername(acosStaffMember.PortalUsername) || c.Emailid == customerEmailId);
            if (tiCustomer != null)
            {
                await CheckExistingCustomerDetail(tiCustomer, acosStaffMember);
            }
            else
            {
                var customer = await CreateCustomerFromStaff(acosStaffMember);
                _tiContext.Customers.Add(customer);
            }
        }

        SetNextCustomerSequence();
        await _tiContext.SaveChangesAsync(cancellationToken);
    }

    private void CheckExistingLocationDetail(Location location, School school)
    {
        if (location.Name != ConvertSchoolNameToLocationName(school.Name))
        {
            location.Name = school.Name;
            _logger.Information("{id}: School: Name {school} - Code {code}: Name updated to {newName}", JobId, school.Name, school.Code, location.Name);
        }

        if (location.Address != school.Address)
        {
            location.Address = school.Address;
            _logger.Information("{id}: School: Name {school} - Code {code}: Address updated to {newAddress}", JobId, school.Name, school.Code, location.Address);
        }

        if (location.City != school.Town)
        {
            location.City = school.Town;
            _logger.Information("{id}: School: Name {school} - Code {code}: City updated to {newCity}", JobId, school.Name, school.Code, location.City);
        }

        if (location.Zip != school.PostCode)
        {
            location.Zip = school.PostCode;
            _logger.Information("{id}: School: Name {school} - Code {code}: PostCode updated to {newPostCode}", JobId, school.Name, school.Code, location.Zip);
        }

        if (location.Phone != school.PhoneNumber)
        {
            location.Phone = school.PhoneNumber;
            _logger.Information("{id}: School: Name {school} - Code {code}: PhoneNumber updated to {newPhone}", JobId, school.Name, school.Code, location.Phone);
        }

        var acc = school.StaffAssignments.FirstOrDefault(s => s.Role == SchoolContactRole.Coordinator)?.SchoolContact;
        if (acc != null)
        {
            location.MainContact = acc.DisplayName;
            location.Maincontctphone = acc.PhoneNumber;

            _logger.Information("{id}: School: Name {school} - Code {code}: ACC updated to {newACC}", JobId, school.Name, school.Code, acc.DisplayName);
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

        _logger.Information("{id}: School: Name {school} - Code {code}: Created new record", JobId, school.Name, school.Code);


        var acc = school.StaffAssignments.FirstOrDefault(s => s.Role == SchoolContactRole.Coordinator)?.SchoolContact;
        if (acc != null)
        {
            location.MainContact = acc.DisplayName;
            location.Maincontctphone = acc.PhoneNumber;

            _logger.Information("{id}: School: Name {school} - Code {code}: ACC updated to {newACC}", JobId, school.Name, school.Code, acc.DisplayName);
        }

        location.Sequence = GetNextLocationSequence();

        return location;
    }

    private static string CreateClientFromPortalUsername(string portalUsername)
    {
        if (portalUsername.Length > 15)
        {
            return portalUsername[..15];
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

        if (locationName.Length > 50 && locationName.Contains(','))
        {
            locationName = locationName[(locationName.IndexOf(",") + 2)..];
            return locationName;
        }

        return locationName;
    }

    private static string ConvertEmailIdToEmail(string emailId)
    {
        return emailId[(emailId.LastIndexOf("}") + 1)..].ToLower();
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

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: FirstName updated to {newName}", JobId, student.DisplayName, student.EmailAddress, student.FirstName);
        }

        if (customer.Name != student.LastName)
        {
            customer.Name = student.LastName;

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, student.DisplayName, student.EmailAddress, student.LastName);
        }

        var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name == "Students");
        customer.Dept = department?.Sequence;

        var location = _tiContext.Locations.FirstOrDefault(c => c.Note == student.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Inactive = 0;

        customer.Updated();
    }

    private async Task CheckExistingCustomerDetail(Customer customer, Staff staff)
    {
        if (customer.Client != CreateClientFromPortalUsername(staff.PortalUsername).ToUpper())
            customer.Client = CreateClientFromPortalUsername(staff.PortalUsername).ToUpper();

        customer.Emailid = ConvertEmailToEmailId(staff.EmailAddress);

        if (customer.Fname != staff.FirstName)
        {
            customer.Fname = staff.FirstName;

            _logger.Information("{id}: Staff: Name {staff} - Email {emailAddress}: FirstName updated to {newName}", JobId, staff.DisplayName, staff.EmailAddress, staff.FirstName);
        }

        if (customer.Name != staff.LastName)
        {
            customer.Name = staff.LastName;

            _logger.Information("{id}: Staff: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, staff.DisplayName, staff.EmailAddress, staff.LastName);
        }

        FacultyMembership membership = staff.Faculties.FirstOrDefault(member => !member.IsDeleted);
        Faculty faculty = await _facultyRepository.GetById(membership.FacultyId);
        if (faculty is not null) 
        {
            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name.Contains(faculty.Name));
            customer.Dept = department?.Sequence;
        }
        
        var location = _tiContext.Locations.FirstOrDefault(c => c.Note == staff.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Inactive = 0;

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

        _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: Created new record", JobId, student.DisplayName, student.EmailAddress);

        var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name == "Students");
        customer.Dept = department?.Sequence;

        var location = _tiContext.Locations.ToList().FirstOrDefault(c => c.Note == student.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Sequence = GetNextCustomerSequence();

        return customer;
    }

    private async Task<Customer> CreateCustomerFromStaff(Staff staff)
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

        _logger.Information("{id}: Staff: Name {staff} - Email {emailAddress}: Created new record", JobId, staff.DisplayName, staff.EmailAddress);

        FacultyMembership membership = staff.Faculties.FirstOrDefault(member => !member.IsDeleted);
        Faculty faculty = await _facultyRepository.GetById(membership.FacultyId);
        if (faculty is not null)
        {
            var department = _tiContext.Departments.ToList().FirstOrDefault(c => c.Name.Contains(faculty.Name));
            customer.Dept = department?.Sequence;
        }

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

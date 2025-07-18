﻿#nullable enable
namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Models.Students.Identifiers;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence.TrackItContext;
using Persistence.TrackItContext.Models;
using System.Globalization;

internal sealed class TrackItSyncJob : ITrackItSyncJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly TrackItContext _tiContext;
    private readonly ILogger _logger;
    private static int _newCustomerSequence;
    private static int _newLocationSequence;
    private List<Faculty> _faculties = new();
    private List<Location> _tiLocations = new();
    private List<Department> _tiDepartments = new();

    private Guid JobId { get; set; }

    public TrackItSyncJob(
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        TrackItContext tiContext, 
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _contactRepository = contactRepository;
        _tiContext = tiContext;
        _logger = logger.ForContext<ITrackItSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        JobId = jobId;

        List<Student> acosStudents = await _studentRepository.GetAll(cancellationToken);
        List<School> acosSchools = await _schoolRepository.GetAllActive(cancellationToken);
        List<StaffMember> acosStaff = await _staffRepository.GetAll(cancellationToken);
        List<SchoolContact> acosContacts = await _contactRepository.GetAllByRole(Position.TimetableOfficer, cancellationToken);
        _faculties = await _facultyRepository.GetAll(cancellationToken);

        List<Customer> tiCustomers = await _tiContext.Customers.ToListAsync(cancellationToken);
        _tiLocations = await _tiContext.Locations.ToListAsync(cancellationToken);
        _tiDepartments = await _tiContext.Departments.ToListAsync(cancellationToken);

        foreach (School acosSchool in acosSchools)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: School: Name {school} - Code {code}", jobId, acosSchool.Name, acosSchool.Code);
            
            Location? tiLocation = _tiLocations.FirstOrDefault(c => c.Note == acosSchool.Code);
            
            if (tiLocation is not null)
            {
                CheckExistingLocationDetail(tiLocation, acosSchool);
            }
            else
            { 
                Location location = CreateLocationFromSchool(acosSchool);
                _tiContext.Locations.Add(location);
            }

            SetNextLocationSequence();
            await _tiContext.SaveChangesAsync(cancellationToken);
        }

        _tiLocations = await _tiContext.Locations.ToListAsync(cancellationToken);

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

            if (emailAddress.Contains("@det.nsw.edu.au", StringComparison.OrdinalIgnoreCase))
            {
                StaffMember? staffMember = acosStaff.FirstOrDefault(staff => staff.EmailAddress.Email.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

                if (staffMember is not null)
                {
                    if (!staffMember.IsDeleted)
                        continue;

                    if (staffMember.DateDeleted != null && staffMember.DateDeleted.Value.AddDays(7) > DateTime.Today)
                        continue;
                }

                SchoolContact? contact = acosContacts.FirstOrDefault(contact => contact.EmailAddress.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

                if (contact is not null)
                {
                    bool validRoles = contact.Assignments
                        .Any(role =>
                            role.Role == Position.TimetableOfficer && 
                            !role.IsDeleted);

                    if (!contact.IsDeleted && validRoles)
                        continue;
                }
                
                _logger.Information("{id}: Customer: {user} no longer active - removing", jobId, emailAddress);

                customer.Inactive = 1;

                await _tiContext.SaveChangesAsync(cancellationToken);
            }

            if (emailAddress.Contains("@education.nsw.gov.au", StringComparison.OrdinalIgnoreCase))
            {
                Student? student = acosStudents.FirstOrDefault(student => student.EmailAddress.Email.Equals(emailAddress, StringComparison.OrdinalIgnoreCase));

                // Student not found. Should be removed.
                if (student is not null)
                {
                    if (!student.IsDeleted)
                        continue;

                    if (student.DeletedAt != DateTime.MinValue && student.DeletedAt.AddDays(7) > DateTime.Today)
                        continue;
                }

                _logger.Information("{id}: Customer: {user} no longer active - removing", jobId, emailAddress);

                customer.Inactive = 1;

                await _tiContext.SaveChangesAsync(cancellationToken);
            }
        }
        
        foreach (Student acosStudent in acosStudents.Where(student => !student.IsDeleted))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            // If student does not have an email address registered, they cannot be created in TrackIt
            if (acosStudent.EmailAddress == EmailAddress.None)
                continue;

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}", jobId, acosStudent.Name.DisplayName, acosStudent.EmailAddress);
            
            string customerEmailId = ConvertEmailToEmailId(acosStudent.EmailAddress);
            Customer? tiCustomer = tiCustomers.FirstOrDefault(c => c.Client == acosStudent.StudentReferenceNumber.Number || c.Emailid == customerEmailId);
            
            if (tiCustomer is not null)
            {
                CheckExistingCustomerDetail(tiCustomer, acosStudent);
            }
            else
            {
                Customer customer = CreateCustomerFromStudent(acosStudent);
                _tiContext.Customers.Add(customer);
            }
        }

        SetNextCustomerSequence();
        await _tiContext.SaveChangesAsync(cancellationToken);

        foreach (StaffMember acosStaffMember in acosStaff.Where(staff => !staff.IsDeleted))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Teacher: Name {teacher} - Email {emailAddress}", jobId, acosStaffMember.Name.PreferredName, acosStaffMember.EmailAddress);

            string customerEmailId = ConvertEmailToEmailId(acosStaffMember.EmailAddress);
            Customer? tiCustomer = tiCustomers.FirstOrDefault(c => acosStaffMember.EmailAddress.Email.Contains(c.Client, StringComparison.InvariantCultureIgnoreCase) || c.Emailid == customerEmailId);
            
            if (tiCustomer is not null)
            {
                CheckExistingCustomerDetail(tiCustomer, acosStaffMember);
            }
            else
            {
                Customer customer = CreateCustomerFromStaff(acosStaffMember);
                _tiContext.Customers.Add(customer);
            }
        }

        SetNextCustomerSequence();
        await _tiContext.SaveChangesAsync(cancellationToken);

        foreach (SchoolContact acosContact in acosContacts.Where(contact => !contact.IsDeleted))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            bool validRoles = acosContact.Assignments
                .Any(role =>
                    role.Role == Position.TimetableOfficer &&
                    !role.IsDeleted);

            if (!validRoles)
                continue;

            _logger.Information("{id}: Contact: Name {contact} - Email {emailAddress}", jobId, acosContact.DisplayName, acosContact.EmailAddress);

            string contactEmailId = ConvertEmailToEmailId(acosContact.EmailAddress);
            string contactPortalId = ConvertEmailToPortalId(acosContact.EmailAddress);

            Customer? tiCustomer = tiCustomers.FirstOrDefault(c => c.Client == contactPortalId || c.Emailid == contactEmailId);

            if (tiCustomer is not null)
            {
                CheckExistingCustomerDetail(tiCustomer, acosContact);
            }
            else
            {
                Customer customer = CreateCustomerFromContact(acosContact);
                _tiContext.Customers.Add(customer);
            }
        }

        SetNextCustomerSequence();
        await _tiContext.SaveChangesAsync(cancellationToken);
    }

    private void CheckExistingLocationDetail(Location location, School school)
    {
        string newName = ConvertSchoolNameToLocationName(school.Name);

        if (location.Name != newName)
        {
            location.Name = newName;
            _logger.Information("{id}: School: Name {school} - Code {code}: Name updated to {newName}", JobId, school.Name, school.Code, newName);
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

        location.MainContact = null;
        location.Maincontctphone = null;

        location.Updated();
    }

    private Location CreateLocationFromSchool(School school)
    {
        Location location = new()
        {
            Note = school.Code,
            Name = ConvertSchoolNameToLocationName(school.Name),
            Address = school.Address,
            City = school.Town,
            Zip = school.PostCode,
            Phone = school.PhoneNumber
        };

        _logger.Information("{id}: School: Name {school} - Code {code}: Created new record", JobId, school.Name, school.Code);

        location.MainContact = null;
        location.Maincontctphone = null;

        location.Sequence = GetNextLocationSequence();

        return location;
    }

    private static string ConvertSchoolNameToLocationName(string schoolName)
    {
        string locationName = schoolName;
        switch (locationName.Length)
        {
            case > 50 when locationName.Contains("Secondary College", StringComparison.OrdinalIgnoreCase):
                locationName = locationName.Replace("Secondary College", "SC", StringComparison.OrdinalIgnoreCase);
                return locationName;
            case > 50 when locationName.Contains("Environmental Education Centre", StringComparison.OrdinalIgnoreCase):
                locationName = locationName.Replace("Environmental Education Centre", "EEC", StringComparison.OrdinalIgnoreCase);
                return locationName;
            case > 50 when locationName.Contains("Creative and Performing Arts", StringComparison.OrdinalIgnoreCase):
                locationName = locationName.Replace("Creative and Performing Arts", "CAPA", StringComparison.OrdinalIgnoreCase);
                return locationName;
            case > 50 when locationName.Contains(',', StringComparison.OrdinalIgnoreCase):
                locationName = locationName[(locationName.IndexOf(',', StringComparison.OrdinalIgnoreCase) + 2)..];
                return locationName;
            default:
                return locationName;
        }
    }

    private static string ConvertEmailIdToEmail(string emailId) =>
        emailId[(emailId.LastIndexOf('}') + 1)..].ToLower(CultureInfo.InvariantCulture);

    private static string ConvertEmailToEmailId(EmailAddress email) =>
        $"SMTP:{{{email.Email.ToLower(CultureInfo.InvariantCulture)}}}{email.Email.ToLower(CultureInfo.InvariantCulture)}";
    
    private static string ConvertEmailToEmailId(string email) =>
        $"SMTP:{{{email.ToLower(CultureInfo.InvariantCulture)}}}{email.ToLower(CultureInfo.InvariantCulture)}";

    private static string ConvertEmailToPortalId(string email) =>
        email[..(email.IndexOf('@') - 1)].ToLower(CultureInfo.InvariantCulture);

    private void CheckExistingCustomerDetail(Customer customer, SchoolContact contact)
    {
        // Is this customer linked to an Aurora College staff member as well?
        Department? checkDepartment = _tiDepartments.FirstOrDefault(c => c.Sequence == customer.Dept);

        if (checkDepartment?.Name.Contains("Faculty", StringComparison.OrdinalIgnoreCase) ?? false)
            return;

        string? contactPortalId = ConvertEmailToPortalId(contact.EmailAddress);

        if (string.IsNullOrWhiteSpace(contactPortalId))
            return;

        string? schoolCode = contact.Assignments
            .FirstOrDefault(role => 
                !role.IsDeleted && 
                role.Role == Position.TimetableOfficer)
            ?.SchoolCode;

        if (!customer.Client.Equals(contactPortalId.ToUpper(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            customer.Client = contactPortalId.ToUpper(CultureInfo.InvariantCulture);

        customer.Emailid = ConvertEmailToEmailId(contact.EmailAddress);

        if (customer.Fname != contact.FirstName)
        {
            customer.Fname = contact.FirstName;

            _logger.Information("{id}: Contact: Name {contact} - Email {emailAddress}: FirstName updated to {newName}", JobId, contact.DisplayName, contact.EmailAddress, contact.FirstName);
        }

        if (customer.Name != contact.LastName)
        {
            customer.Name = contact.LastName;

            _logger.Information("{id}: Contact: Name {contact} - Email {emailAddress}: LastName updated to {newName}", JobId, contact.DisplayName, contact.EmailAddress, contact.LastName);
        }

        Department? department = _tiDepartments.FirstOrDefault(c => c.Name == "Partner School");
        customer.Dept = department?.Sequence;

        if (!string.IsNullOrWhiteSpace(schoolCode))
        {
            Location? location = _tiLocations.FirstOrDefault(c => c.Note == schoolCode);
            customer.Location = location?.Sequence;
        }
        
        customer.Inactive = 0;

        customer.Updated();
    }

    private void CheckExistingCustomerDetail(Customer customer, Student student)
    {
        if (!customer.Client.Equals(student.StudentReferenceNumber.Number.ToUpper(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            customer.Client = student.StudentReferenceNumber.Number.ToUpper(CultureInfo.InvariantCulture);

        customer.Emailid = ConvertEmailToEmailId(student.EmailAddress);

        if (customer.Fname != student.Name.PreferredName)
        {
            customer.Fname = student.Name.PreferredName;

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: FirstName updated to {newName}", JobId, student.Name.DisplayName, student.EmailAddress, student.Name.PreferredName);
        }

        if (customer.Name != student.Name.LastName)
        {
            customer.Name = student.Name.LastName;

            _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, student.Name.DisplayName, student.EmailAddress, student.Name.LastName);
        }

        Department? department = _tiDepartments.FirstOrDefault(c => c.Name == "Students");
        customer.Dept = department?.Sequence;

        Location? location = _tiLocations.FirstOrDefault(c => c.Note == student.CurrentEnrolment?.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Inactive = 0;

        customer.Updated();
    }

    private void CheckExistingCustomerDetail(Customer customer, StaffMember staff)
    {
        string staffShortId = staff.EmailAddress.Email.Split('@')[0].ToUpper(CultureInfo.InvariantCulture);

        if (!customer.Client.Equals(staffShortId, StringComparison.OrdinalIgnoreCase))
            customer.Client = staffShortId;

        customer.Emailid = ConvertEmailToEmailId(staff.EmailAddress);

        if (customer.Fname != staff.Name.PreferredName)
        {
            customer.Fname = staff.Name.PreferredName;

            _logger.Information("{id}: Staff: Name {staff} - Email {emailAddress}: FirstName updated to {newName}", JobId, staff.Name.DisplayName, staff.EmailAddress.Email, staff.Name.PreferredName);
        }

        if (customer.Name != staff.Name.LastName)
        {
            customer.Name = staff.Name.LastName;

            _logger.Information("{id}: Staff: Name {student} - Email {emailAddress}: LastName updated to {newName}", JobId, staff.Name.DisplayName, staff.EmailAddress, staff.Name.LastName);
        }

        Faculty? faculty = _faculties.FirstOrDefault(faculty => faculty.Members.Any(member => member.StaffId == staff.Id && !member.IsDeleted));

        if (faculty is not null) 
        {
            Department? department = _tiDepartments.FirstOrDefault(c => c.Name.Contains(faculty.Name, StringComparison.OrdinalIgnoreCase));
            customer.Dept = department?.Sequence;
        }
        
        Location? location = _tiLocations.FirstOrDefault(c => c.Note == staff.CurrentAssignment?.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Inactive = 0;

        customer.Updated();
    }

    private Customer CreateCustomerFromContact(SchoolContact contact)
    {
        string? contactPortalId = ConvertEmailToPortalId(contact.EmailAddress);
        
        Customer customer = new()
        {
            Client = contactPortalId?.ToUpper(CultureInfo.InvariantCulture),
            Emailid = ConvertEmailToEmailId(contact.EmailAddress),
            Group = 2,
            Inactive = 0,
            Fname = contact.FirstName,
            Name = contact.LastName
        };

        _logger.Information("{id}: Contact: Name {contact} - Email {emailAddress}: Created new record", JobId, contact.DisplayName, contact.EmailAddress);

        Department? department = _tiDepartments.FirstOrDefault(c => c.Name == "Partner School");
        customer.Dept = department?.Sequence;

        string? schoolCode = contact.Assignments
            .FirstOrDefault(role =>
                !role.IsDeleted &&
                role.Role == Position.TimetableOfficer)
            ?.SchoolCode;

        if (!string.IsNullOrWhiteSpace(schoolCode))
        {
            Location? location = _tiLocations.FirstOrDefault(c => c.Note == schoolCode);
            customer.Location = location?.Sequence;
        }
        
        customer.Sequence = GetNextCustomerSequence();

        return customer;
    }

    private Customer CreateCustomerFromStudent(Student student)
    {
        Customer customer = new()
        {
            Client = student.StudentReferenceNumber.Number.ToUpper(CultureInfo.InvariantCulture),
            Emailid = ConvertEmailToEmailId(student.EmailAddress),
            Group = 2,
            Inactive = 0,
            Fname = student.Name.PreferredName,
            Name = student.Name.LastName
        };

        _logger.Information("{id}: Student: Name {student} - Email {emailAddress}: Created new record", JobId, student.Name.DisplayName, student.EmailAddress);

        Department? department = _tiDepartments.FirstOrDefault(c => c.Name == "Students");
        customer.Dept = department?.Sequence;

        Location? location = _tiLocations.FirstOrDefault(c => c.Note == student.CurrentEnrolment?.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Sequence = GetNextCustomerSequence();

        return customer;
    }

    private Customer CreateCustomerFromStaff(StaffMember staff)
    {
        string staffShortId = staff.EmailAddress.Email.Split('@')[0].ToUpper(CultureInfo.InvariantCulture);

        Customer customer = new()
        {
            Client = staffShortId,
            Emailid = ConvertEmailToEmailId(staff.EmailAddress),
            Group = 2,
            Inactive = 0,
            Fname = staff.Name.PreferredName,
            Name = staff.Name.LastName
        };

        _logger.Information("{id}: Teacher: Name {staff} - Email {emailAddress}: Created new record", JobId, staff.Name.DisplayName, staff.EmailAddress);

        Faculty? faculty = _faculties.FirstOrDefault(faculty => faculty.Members.Any(member => member.StaffId == staff.Id && !member.IsDeleted));

        if (faculty is not null)
        {
            Department? department = _tiDepartments.FirstOrDefault(c => c.Name.Contains(faculty.Name, StringComparison.OrdinalIgnoreCase));
            customer.Dept = department?.Sequence;
        }

        Location? location = _tiLocations.FirstOrDefault(c => c.Note == staff.CurrentAssignment?.SchoolCode);
        customer.Location = location?.Sequence;

        customer.Sequence = GetNextCustomerSequence();

        return customer;
    }

    private int GetNextCustomerSequence()
    {
        if (_newCustomerSequence == 0)
        {
            int current = _tiContext.Indexes.First(c => c.Name == "_CUSTOMER_").Recnum;

            current++;

            _newCustomerSequence = current;

            return current;
        }

        _newCustomerSequence++;

        return _newCustomerSequence;
    }

    private void SetNextCustomerSequence()
    {
        Index current = _tiContext.Indexes.First(c => c.Name == "_CUSTOMER_");

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
            int current = _tiContext.Indexes.First(c => c.Name == "_LOCATION_").Recnum;

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
        Index current = _tiContext.Indexes.First(c => c.Name == "_LOCATION_");

        if (_newLocationSequence > current.Recnum)
        {
            current.Recnum = _newLocationSequence;
            current.Lastmodified = DateTime.Now;
        }
    }
}

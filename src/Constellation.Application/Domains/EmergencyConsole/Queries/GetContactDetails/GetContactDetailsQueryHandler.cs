namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetContactDetails;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Families;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetContactDetailsQueryHandler
: IQueryHandler<GetContactDetailsQuery, List<ContactDetail>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger _logger;

    public GetContactDetailsQueryHandler(
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IFamilyRepository familyRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _familyRepository = familyRepository;
        _logger = logger
            .ForContext<GetContactDetailsQuery>();
    }

    public async Task<Result<List<ContactDetail>>> Handle(GetContactDetailsQuery request, CancellationToken cancellationToken)
    {
        List<ContactDetail> contacts = [];

        // Get Staff
        List<StaffMember> staffMembers = await _staffRepository.GetAllActive(cancellationToken);
        List<Faculty> facultyMemberships = await _facultyRepository.GetAll(cancellationToken);
        
        foreach (StaffMember member in staffMembers)
        {
            List<string> faculties = facultyMemberships
                .Where(faculty =>
                    faculty.Members
                        .Any(entry => 
                            !entry.IsDeleted && 
                            entry.StaffId == member.Id))
                .Select(faculty => faculty.Name)
                .ToList();

            ContactDetail detail = new(
                member.Name,
                string.Join(", ", faculties),
                ContactDetail.ContactCategory.Staff,
                member.PhoneNumber,
                member.EmailAddress);

            contacts.Add(detail);
        }

        // Get ACCs
        List<SchoolContact> coordinators = await _contactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);

        foreach (SchoolContact coordinator in coordinators)
        {
            SchoolContactRole? assignment = coordinator.Assignments
                .FirstOrDefault(entry =>
                    !entry.IsDeleted &&
                    entry.Role == Position.Coordinator);

            ContactDetail detail = new(
                coordinator.Name,
                assignment?.SchoolName ?? string.Empty,
                ContactDetail.ContactCategory.Coordinator,
                coordinator.PhoneNumber,
                coordinator.EmailAddress);

            contacts.Add(detail);
        }
        
        // Get Parents
        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);
        
        foreach (Family family in families)
        {
            List<Name> familyStudents = students
                .Where(student => 
                    family.Students
                        .Any(entry => entry.StudentId == student.Id))
                .Select(student => student.Name)
                .ToList();

            foreach (Parent parent in family.Parents)
            {
                ContactDetail detail = new(
                    parent.Name,
                    string.Join(", ", familyStudents),
                    ContactDetail.ContactCategory.Parent,
                    parent.MobileNumber,
                    parent.EmailAddress);

                contacts.Add(detail);
            }

            Result<Name> familyName = Name.Create(
                family.FamilyTitle.Split(' ')[0], string.Empty,
                string.Join(' ', family.FamilyTitle.Split(' ')[1..]));
            Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

            if (familyName.IsSuccess && familyEmail.IsSuccess)
            {
                ContactDetail familyDetail = new(
                    familyName.Value,
                    string.Join(", ", familyStudents),
                    ContactDetail.ContactCategory.Parent,
                    PhoneNumber.Empty,
                    familyEmail.Value);

                contacts.Add(familyDetail);
            }
        }

        return contacts;
    }
}

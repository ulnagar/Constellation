namespace Constellation.Infrastructure.Services;

using Application.Domains.Contacts.Models;
using Application.Extensions;
using Application.Models.Auth;
using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Application.Models.Identity.Repositories;
using Constellation.Core.Models.Students.Identifiers;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Messaging.EmergencyConsole.Services;
using Core.Models.Messaging.Enums;
using Core.Models.Offerings;
using Core.Models.Offerings.Enums;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class EmergencyRecipientService : IEmergencyRecipientService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IPeriodRepository _periodRepository;

    public EmergencyRecipientService(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        IEnrolmentRepository enrolmentRepository,
        IDateTimeProvider dateTime,
        IOfferingRepository offeringRepository,
        IIdentityRepository identityRepository,
        IPeriodRepository periodRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _enrolmentRepository = enrolmentRepository;
        _dateTime = dateTime;
        _offeringRepository = offeringRepository;
        _identityRepository = identityRepository;
        _periodRepository = periodRepository;
    }

    public async Task<List<ContactResponse>> GetSelectedRecipientsFromGroup(
        RecipientGroup group,
        CancellationToken cancellationToken = default)
    {
        List<ContactResponse> recipients = [];

        List<StaffMember> staffMembers = await _staffRepository.GetAllActive(cancellationToken);
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);
        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrent(cancellationToken);
        List<SchoolContact> contacts = await _schoolContactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);
        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

        Result<Name> noStudentName = Name.Create("Not Applicable");

        if (group == RecipientGroup.AllStaff)
        {

            foreach (StaffMember member in staffMembers)
            {
                ContactResponse recipient = new(
                    StudentReferenceNumber.Empty,
                    noStudentName.Value,
                    Grade.SpecialProgram,
                    string.Empty,
                    ContactCategory.AuroraTeacher,
                    member.Id,
                    member.Name.DisplayName,
                    member.EmailAddress,
                    member.PhoneNumber,
                    string.Empty);
                
                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllExecStaff)
        {
            List<AppUser> execUsers = await _identityRepository.GetUsersWithTransientClaim(AuthPermission.Messaging_EmergencyConsole_Edit, cancellationToken);

            List<StaffId> staffIds = execUsers
                .SelectMany(user => 
                    user.Links.Where(link => 
                        !link.IsDeleted && 
                        link.Type == LinkType.Staff))
                .Select(link => StaffId.FromValue(link.LinkId))
                .ToList();

            List<StaffMember> execStaffMembers = staffMembers
                .Where(entry => staffIds.Contains(entry.Id))
                .ToList();

            foreach (StaffMember member in execStaffMembers)
            {
                ContactResponse recipient = new(
                    StudentReferenceNumber.Empty,
                    noStudentName.Value,
                    Grade.SpecialProgram,
                    string.Empty,
                    ContactCategory.AuroraTeacher,
                    member.Id,
                    member.Name.DisplayName,
                    member.EmailAddress,
                    member.PhoneNumber,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllTeachersOnClassNow)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> currentPeriodIds = todaysPeriods.Where(period => 
                period.StartTime <= _dateTime.Now.TimeOfDay &&
                period.EndTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(currentPeriodIds, cancellationToken);

            List<StaffId> teacherIds = currentOfferings.SelectMany(offering => 
                offering.Teachers
                    .Where(teacher => teacher.Type == AssignmentType.ClassroomTeacher)
                    .Select(teacher => teacher.StaffId))
                .ToList();

            List<StaffMember> currentClassTeachers = staffMembers
                .Where(entry => teacherIds.Contains(entry.Id))
                .ToList();

            foreach (StaffMember member in currentClassTeachers)
            {
                ContactResponse recipient = new(
                    StudentReferenceNumber.Empty,
                    noStudentName.Value,
                    Grade.SpecialProgram,
                    string.Empty,
                    ContactCategory.AuroraTeacher,
                    member.Id,
                    member.Name.DisplayName,
                    member.EmailAddress,
                    member.PhoneNumber,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllTeachersOnClassRestOfDay)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> restOfDayPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(restOfDayPeriodIds, cancellationToken);

            List<StaffId> teacherIds = currentOfferings.SelectMany(offering =>
                    offering.Teachers
                        .Where(teacher => teacher.Type == AssignmentType.ClassroomTeacher)
                        .Select(teacher => teacher.StaffId))
                .ToList();

            List<StaffMember> todayClassTeachers = staffMembers
                .Where(entry => teacherIds.Contains(entry.Id))
                .ToList();

            foreach (StaffMember member in todayClassTeachers)
            {
                ContactResponse recipient = new(
                    StudentReferenceNumber.Empty,
                    noStudentName.Value,
                    Grade.SpecialProgram,
                    string.Empty,
                    ContactCategory.AuroraTeacher,
                    member.Id,
                    member.Name.DisplayName,
                    member.EmailAddress,
                    member.PhoneNumber,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllStudents)
        {
            foreach (Student student in students)
            {
                SchoolEnrolment? enrolment = student.CurrentEnrolment;

                ContactResponse recipient = new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment?.Grade ?? Grade.SpecialProgram,
                    enrolment?.SchoolName ?? string.Empty,
                    ContactCategory.Student,
                    student.Id,
                    student.Name.DisplayName,
                    student.EmailAddress,
                    null,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllStudentsOnClassNow)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> currentPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime <= _dateTime.Now.TimeOfDay &&
                    period.EndTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(currentPeriodIds, cancellationToken);

            List<Student> currentClassStudents = [];

            foreach (Offering offering in currentOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                currentClassStudents.AddRange(classStudents);

                currentClassStudents = currentClassStudents.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in currentClassStudents)
            {
                SchoolEnrolment? enrolment = student.CurrentEnrolment;

                ContactResponse recipient = new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment?.Grade ?? Grade.SpecialProgram,
                    enrolment?.SchoolName ?? string.Empty,
                    ContactCategory.Student,
                    student.Id,
                    student.Name.DisplayName,
                    student.EmailAddress,
                    null,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllStudentsOnClassRestOfDay)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> restOfDayPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> todaysOfferings = await _offeringRepository.GetFromListOfPeriodIds(restOfDayPeriodIds, cancellationToken);

            List<Student> todaysClassStudents = [];

            foreach (Offering offering in todaysOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                todaysClassStudents.AddRange(classStudents);

                todaysClassStudents = todaysClassStudents.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in todaysClassStudents)
            {
                SchoolEnrolment? enrolment = student.CurrentEnrolment;

                ContactResponse recipient = new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment?.Grade ?? Grade.SpecialProgram,
                    enrolment?.SchoolName ?? string.Empty,
                    ContactCategory.Student,
                    student.Id,
                    student.Name.DisplayName,
                    student.EmailAddress,
                    null,
                    string.Empty);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllACCs)
        {
            foreach (SchoolContact contact in contacts)
            {
                foreach (SchoolContactRole assignment in contact.Assignments.Where(entry => !entry.IsDeleted))
                {
                    Result<Name> schoolName = Name.Create(assignment.SchoolName);

                    if (schoolName.IsFailure)
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            noStudentName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                    else
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            schoolName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                }
            }
        }

        if (group == RecipientGroup.AllACCsWithStudentsOnClassNow)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> currentPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime <= _dateTime.Now.TimeOfDay &&
                    period.EndTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(currentPeriodIds, cancellationToken);

            List<Student> currentClassStudents = [];

            foreach (Offering offering in currentOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                currentClassStudents.AddRange(classStudents);

                currentClassStudents = currentClassStudents.DistinctBy(student => student.Id).ToList();
            }

            List<SchoolCode> schoolCodes = currentClassStudents
                .Select(student => student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty)
                .ToList();

             List<SchoolContact> currentClassCoordinators = contacts
                .Where(contact => 
                    !contact.IsDeleted &&
                    contact.Assignments.Any(role =>
                        !role.IsDeleted && 
                        role.Role == Position.Coordinator && 
                        schoolCodes.Contains(role.SchoolCode)))
                .ToList();

            foreach (SchoolContact contact in currentClassCoordinators)
            {
                foreach (SchoolContactRole assignment in contact.Assignments.Where(entry => !entry.IsDeleted))
                {
                    Result<Name> schoolName = Name.Create(assignment.SchoolName);

                    if (schoolName.IsFailure)
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            noStudentName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                    else
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            schoolName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                }
            }
        }

        if (group == RecipientGroup.AllACCsWithStudentsOnClassRestOfDay)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> restOfDayPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(restOfDayPeriodIds, cancellationToken);

            List<Student> todaysClassStudents = [];

            foreach (Offering offering in currentOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                todaysClassStudents.AddRange(classStudents);

                todaysClassStudents = todaysClassStudents.DistinctBy(student => student.Id).ToList();
            }

            List<SchoolCode> schoolCodes = todaysClassStudents.Select(student => student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty).ToList();
            
            List<SchoolContact> todaysClassCoordinators = contacts
                .Where(contact =>
                    !contact.IsDeleted &&
                    contact.Assignments.Any(role =>
                        !role.IsDeleted &&
                        role.Role == Position.Coordinator &&
                        schoolCodes.Contains(role.SchoolCode)))
                .ToList();

            foreach (SchoolContact contact in todaysClassCoordinators)
            {
                foreach (SchoolContactRole assignment in contact.Assignments.Where(entry => !entry.IsDeleted))
                {
                    Result<Name> schoolName = Name.Create(assignment.SchoolName);

                    if (schoolName.IsFailure)
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            noStudentName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                    else
                    {
                        ContactResponse recipient = new(
                            StudentReferenceNumber.Empty,
                            schoolName.Value,
                            Grade.SpecialProgram,
                            assignment.SchoolName,
                            ContactCategory.PartnerSchoolACC,
                            contact.Id,
                            contact.Name.DisplayName,
                            contact.EmailAddress,
                            contact.PhoneNumber.IsMobile() ? contact.PhoneNumber : null,
                            string.Empty);

                        recipients.Add(recipient);
                    }
                }
            }
        }

        if (group == RecipientGroup.AllParents)
        {
            foreach (Family family in families)
            {
                if (family.Students.Any(entry => !entry.IsResidentialFamily))
                    continue;

                Result<Name> familyName = Name.Create(family.FamilyTitle);
                Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                if (familyName.IsSuccess && familyEmail.IsSuccess)
                {
                    foreach (var studentLink in family.Students)
                    {
                        Student? student = students.FirstOrDefault(entry => entry.Id == studentLink.StudentId);

                        if (student is null)
                            continue;

                        ContactResponse recipient = new(
                            student.StudentReferenceNumber,
                            student.Name,
                            student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                            student.CurrentEnrolment?.SchoolName ?? string.Empty,
                            ContactCategory.ResidentialFamily,
                            family.Id,
                            familyName.Value,
                            familyEmail.Value,
                            null,
                            string.Empty);

                        recipients.Add(recipient);

                        foreach (Parent parent in family.Parents)
                        {
                            ContactCategory category = parent.SentralLink switch
                            {
                                Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                                Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                                _ => ContactCategory.ResidentialFamily
                            };

                            recipients.Add(new(
                                student.StudentReferenceNumber,
                                student.Name,
                                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                                student.CurrentEnrolment?.SchoolName ?? string.Empty,
                                category,
                                parent.Id,
                                parent.Name.DisplayName,
                                parent.EmailAddress,
                                parent.MobileNumber.IsMobile() ? parent.MobileNumber : null,
                                string.Empty));
                        }
                    }
                }
            }
        }

        if (group == RecipientGroup.AllParentsOnClassNow)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> currentPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime <= _dateTime.Now.TimeOfDay &&
                    period.EndTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(currentPeriodIds, cancellationToken);

            List<Student> currentClassStudents = [];

            foreach (Offering offering in currentOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                currentClassStudents.AddRange(classStudents);

                currentClassStudents = currentClassStudents.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in currentClassStudents)
            {
                List<Family> studentFamilies = families
                    .Where(entry => entry.Students.Any(link => link.StudentId == student.Id))
                    .ToList();

                foreach (Family family in studentFamilies)
                {
                    if (family.Students.Any(entry => !entry.IsResidentialFamily))
                        continue;

                    Result<Name> familyName = Name.Create(family.FamilyTitle);
                    Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                    if (familyName.IsSuccess && familyEmail.IsSuccess)
                    {
                        ContactResponse recipient = new(
                            student.StudentReferenceNumber,
                            student.Name,
                            student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                            student.CurrentEnrolment?.SchoolName ?? string.Empty,
                            ContactCategory.ResidentialFamily,
                            family.Id,
                            familyName.Value,
                            familyEmail.Value,
                            null,
                            string.Empty);

                        recipients.Add(recipient);

                        foreach (Parent parent in family.Parents)
                        {
                            ContactCategory category = parent.SentralLink switch
                            {
                                Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                                Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                                _ => ContactCategory.ResidentialFamily
                            };

                            recipients.Add(new(
                                student.StudentReferenceNumber,
                                student.Name,
                                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                                student.CurrentEnrolment?.SchoolName ?? string.Empty,
                                category,
                                parent.Id,
                                parent.Name.DisplayName,
                                parent.EmailAddress,
                                parent.MobileNumber.IsMobile() ? parent.MobileNumber : null,
                                string.Empty));
                        }
                    }
                }
            }
        }

        if (group == RecipientGroup.AllParentsOnClassRestOfDay)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> currentPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime <= _dateTime.Now.TimeOfDay &&
                    period.EndTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(currentPeriodIds, cancellationToken);

            List<Student> todaysClassStudents = [];

            foreach (Offering offering in currentOfferings)
            {
                List<StudentId> classStudentIds = enrolments
                    .Where(entry =>
                        entry is OfferingEnrolment { IsDeleted: false } enrolment
                        && enrolment.OfferingId == offering.Id)
                    .Select(entry => entry.StudentId)
                    .ToList();

                List<Student> classStudents = students
                    .Where(entry => classStudentIds.Contains(entry.Id))
                    .ToList();

                todaysClassStudents.AddRange(classStudents);

                todaysClassStudents = todaysClassStudents.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in todaysClassStudents)
            {
                List<Family> studentFamilies = families
                    .Where(entry => entry.Students.Any(link => link.StudentId == student.Id))
                    .ToList();

                foreach (Family family in studentFamilies)
                {
                    if (family.Students.Any(entry => !entry.IsResidentialFamily))
                        continue;

                    Result<Name> familyName = Name.Create(family.FamilyTitle);
                    Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                    if (familyName.IsSuccess && familyEmail.IsSuccess)
                    {
                        ContactResponse recipient = new(
                            student.StudentReferenceNumber,
                            student.Name,
                            student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                            student.CurrentEnrolment?.SchoolName ?? string.Empty,
                            ContactCategory.ResidentialFamily,
                            family.Id,
                            familyName.Value,
                            familyEmail.Value,
                            null,
                            string.Empty);

                        recipients.Add(recipient);

                        foreach (Parent parent in family.Parents)
                        {
                            ContactCategory category = parent.SentralLink switch
                            {
                                Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                                Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                                _ => ContactCategory.ResidentialFamily
                            };

                            recipients.Add(new(
                                student.StudentReferenceNumber,
                                student.Name,
                                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                                student.CurrentEnrolment?.SchoolName ?? string.Empty,
                                category,
                                parent.Id,
                                parent.Name.DisplayName,
                                parent.EmailAddress,
                                parent.MobileNumber.IsMobile() ? parent.MobileNumber : null,
                                string.Empty));
                        }
                    }
                }
            }
        }

        recipients = recipients.Distinct().ToList();

        return recipients;
    }
}

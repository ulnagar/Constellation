namespace Constellation.Infrastructure.Services;

using Application.Extensions;
using Application.Models.Auth;
using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Application.Models.Identity.Repositories;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Models.EmergencyConsole.Services;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Offerings;
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
    private readonly IDateTimeProvider _dateTime;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IPeriodRepository _periodRepository;

    public EmergencyRecipientService(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        IDateTimeProvider dateTime,
        IOfferingRepository offeringRepository,
        IIdentityRepository identityRepository,
        IPeriodRepository periodRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _dateTime = dateTime;
        _offeringRepository = offeringRepository;
        _identityRepository = identityRepository;
        _periodRepository = periodRepository;
    }

    public async Task<List<AlertRecipient>> GetSelectedRecipientsFromGroup(
        RecipientGroup group,
        CancellationToken cancellationToken = default)
    {
        List<AlertRecipient> recipients = [];

        if (group == RecipientGroup.AllStaff)
        {
            List<StaffMember> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                AlertRecipient recipient = AlertRecipient.Create(member.Name, member.EmailAddress, member.PhoneNumber);
                
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

            List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                AlertRecipient recipient = AlertRecipient.Create(member.Name, member.EmailAddress, member.PhoneNumber);

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

            List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                AlertRecipient recipient = AlertRecipient.Create(member.Name, member.EmailAddress, member.PhoneNumber);

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

            List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                AlertRecipient recipient = AlertRecipient.Create(member.Name, member.EmailAddress, member.PhoneNumber);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllStudents)
        {
            List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

            foreach (Student student in students)
            {
                AlertRecipient recipient = AlertRecipient.Create(student.Name, student.EmailAddress);

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

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in students)
            {
                AlertRecipient recipient = AlertRecipient.Create(student.Name, student.EmailAddress);

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

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(restOfDayPeriodIds, cancellationToken);

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in students)
            {
                AlertRecipient recipient = AlertRecipient.Create(student.Name, student.EmailAddress);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllACCs)
        {
            List<SchoolContact> contacts = await _schoolContactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
                AlertRecipient recipient = AlertRecipient.Create(contact.Name, contact.EmailAddress, contact.PhoneNumber);

                recipients.Add(recipient);
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

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            List<SchoolCode> schoolCodes = students.Select(student => student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty).ToList();

            List<SchoolContact> contacts = await _schoolContactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);

            contacts = contacts
                .Where(contact => 
                    contact.Assignments.Any(role =>
                        !role.IsDeleted && 
                        role.Role == Position.Coordinator && 
                        schoolCodes.Contains(role.SchoolCode)))
                .ToList();

            foreach (SchoolContact contact in contacts)
            {
                AlertRecipient recipient = AlertRecipient.Create(contact.Name, contact.EmailAddress, contact.PhoneNumber);

                recipients.Add(recipient);
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

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            List<SchoolCode> schoolCodes = students.Select(student => student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty).ToList();

            List<SchoolContact> contacts = await _schoolContactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);

            contacts = contacts
                .Where(contact =>
                    contact.Assignments.Any(role =>
                        !role.IsDeleted &&
                        role.Role == Position.Coordinator &&
                        schoolCodes.Contains(role.SchoolCode)))
                .ToList();

            foreach (SchoolContact contact in contacts)
            {
                AlertRecipient recipient = AlertRecipient.Create(contact.Name, contact.EmailAddress, contact.PhoneNumber);

                recipients.Add(recipient);
            }
        }

        if (group == RecipientGroup.AllParents)
        {
            List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

            foreach (Family family in families)
            {
                if (family.Students.Any(entry => !entry.IsResidentialFamily))
                    continue;

                Result<Name> familyName = Name.Create(
                    family.FamilyTitle.Split(' ')[0], string.Empty,
                    string.Join(' ', family.FamilyTitle.Split(' ')[1..]));
                Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                if (familyName.IsSuccess && familyEmail.IsSuccess)
                {
                    AlertRecipient familyRecipient = AlertRecipient.Create(familyName.Value, familyEmail.Value);
                    recipients.Add(familyRecipient);
                }

                foreach (Parent parent in family.Parents)
                {
                    if (parent.EmailAddress != EmailAddress.None && parent.MobileNumber != PhoneNumber.Empty)
                    {
                        AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress, parent.MobileNumber);
                        recipients.Add(parentRecipient);
                    }
                    else if (parent.MobileNumber != PhoneNumber.Empty)
                    {
                        AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.MobileNumber);
                        recipients.Add(parentRecipient);
                    }
                    else if (parent.EmailAddress != EmailAddress.None)
                    {
                        AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress);
                        recipients.Add(parentRecipient);
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

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in students)
            {
                List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

                foreach (Family family in families)
                {
                    if (family.Students.Any(entry => !entry.IsResidentialFamily))
                        continue;

                    Result<Name> familyName = Name.Create(
                        family.FamilyTitle.Split(' ')[0], string.Empty,
                        string.Join(' ', family.FamilyTitle.Split(' ')[1..]));
                    Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                    if (familyName.IsSuccess && familyEmail.IsSuccess)
                    {
                        AlertRecipient familyRecipient = AlertRecipient.Create(familyName.Value, familyEmail.Value);
                        recipients.Add(familyRecipient);
                    }

                    foreach (Parent parent in family.Parents)
                    {
                        if (parent.EmailAddress != EmailAddress.None && parent.MobileNumber != PhoneNumber.Empty)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress, parent.MobileNumber);
                            recipients.Add(parentRecipient);
                        }
                        else if (parent.MobileNumber != PhoneNumber.Empty)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.MobileNumber);
                            recipients.Add(parentRecipient);
                        }
                        else if (parent.EmailAddress != EmailAddress.None)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress);
                            recipients.Add(parentRecipient);
                        }
                    }
                }
            }
        }

        if (group == RecipientGroup.AllParentsOnClassRestOfDay)
        {
            int dayNumber = _dateTime.Today.GetDayNumber();

            List<Period> todaysPeriods = await _periodRepository.GetByDayNumber(dayNumber, cancellationToken);

            List<PeriodId> restOfDayPeriodIds = todaysPeriods.Where(period =>
                    period.StartTime >= _dateTime.Now.TimeOfDay)
                .Select(period => period.Id)
                .ToList();

            List<Offering> currentOfferings = await _offeringRepository.GetFromListOfPeriodIds(restOfDayPeriodIds, cancellationToken);

            List<Student> students = [];

            foreach (Offering offering in currentOfferings)
            {
                List<Student> classStudents = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

                students.AddRange(classStudents);

                students = students.DistinctBy(student => student.Id).ToList();
            }

            foreach (Student student in students)
            {
                List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

                foreach (Family family in families)
                {
                    if (family.Students.Any(entry => !entry.IsResidentialFamily))
                        continue;

                    Result<Name> familyName = Name.Create(
                        family.FamilyTitle.Split(' ')[0], string.Empty,
                        string.Join(' ', family.FamilyTitle.Split(' ')[1..]));
                    Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

                    if (familyName.IsSuccess && familyEmail.IsSuccess)
                    {
                        AlertRecipient familyRecipient = AlertRecipient.Create(familyName.Value, familyEmail.Value);
                        recipients.Add(familyRecipient);
                    }

                    foreach (Parent parent in family.Parents)
                    {
                        if (parent.EmailAddress != EmailAddress.None && parent.MobileNumber != PhoneNumber.Empty)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress, parent.MobileNumber);
                            recipients.Add(parentRecipient);
                        }
                        else if (parent.MobileNumber != PhoneNumber.Empty)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.MobileNumber);
                            recipients.Add(parentRecipient);
                        }
                        else if (parent.EmailAddress != EmailAddress.None)
                        {
                            AlertRecipient parentRecipient = AlertRecipient.Create(parent.Name, parent.EmailAddress);
                            recipients.Add(parentRecipient);
                        }
                    }
                }
            }
        }

        recipients = recipients.Distinct().ToList();

        return recipients;
    }
}

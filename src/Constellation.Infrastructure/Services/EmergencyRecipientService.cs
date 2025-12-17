namespace Constellation.Infrastructure.Services;

using Application.Extensions;
using Application.Models.Auth;
using Application.Models.Identity;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Models.EmergencyConsole.Services;
using Core.Models.Families;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

internal sealed class EmergencyRecipientService : IEmergencyRecipientService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IDateTimeProvider _dateTime;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;

    public EmergencyRecipientService(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository,
        UserManager<AppUser> userManager,
        IDateTimeProvider dateTime,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
        _userManager = userManager;
        _dateTime = dateTime;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
    }

    public async Task<List<EmailRecipient>> GetSelectedEmailRecipientsFromGroup(
        RecipientGroup group,
        CancellationToken cancellationToken = default)
    {
        List<EmailRecipient> recipients = [];

        if (group == RecipientGroup.AllStaff)
        {
            List<StaffMember> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }

        if (group == RecipientGroup.AllExecStaff)
        {
            IList<AppUser> execUsers = await _userManager.GetUsersInRoleAsync(AuthRoles.ExecStaffMember);

            List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(execUsers.Select(user => user.StaffId).ToList(), cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }

        if (group == RecipientGroup.AllStudents)
        {
            List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

            foreach (Student student in students)
            {
                Result<EmailRecipient> recipient = student.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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
                Result<EmailRecipient> recipient = student.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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
                Result<EmailRecipient> recipient = student.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }

        if (group == RecipientGroup.AllACCs)
        {
            List<SchoolContact> contacts = await _schoolContactRepository.GetActiveByRole(Position.Coordinator, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
                Result<EmailRecipient> recipient = contact.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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

            List<string> schoolCodes = students.Select(student => student.CurrentEnrolment?.SchoolCode ?? string.Empty).ToList();

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
                Result<EmailRecipient> recipient = contact.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
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

            List<string> schoolCodes = students.Select(student => student.CurrentEnrolment?.SchoolCode ?? string.Empty).ToList();

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
                Result<EmailRecipient> recipient = contact.GetEmailRecipient();

                if (recipient.IsFailure)
                    continue;

                recipients.Add(recipient.Value);
            }
        }

        if (group == RecipientGroup.AllParents)
        {
            List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

            foreach (Family family in families)
            {
                if (family.Students.Any(entry => !entry.IsResidentialFamily))
                    continue;

                Result<EmailRecipient> recipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (recipient.IsSuccess)
                    recipients.Add(recipient.Value);

                foreach (Parent parent in family.Parents)
                {
                    Result<EmailRecipient> parentRecipient = EmailRecipient.Create($"{parent.FirstName} {parent.LastName}", parent.EmailAddress);

                    if (recipient.IsSuccess)
                        recipients.Add(recipient.Value);
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

                    Result<EmailRecipient> recipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                    if (recipient.IsSuccess)
                        recipients.Add(recipient.Value);

                    foreach (Parent parent in family.Parents)
                    {
                        Result<EmailRecipient> parentRecipient = EmailRecipient.Create($"{parent.FirstName} {parent.LastName}", parent.EmailAddress);

                        if (recipient.IsSuccess)
                            recipients.Add(recipient.Value);
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

                    Result<EmailRecipient> recipient = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                    if (recipient.IsSuccess)
                        recipients.Add(recipient.Value);

                    foreach (Parent parent in family.Parents)
                    {
                        Result<EmailRecipient> parentRecipient = EmailRecipient.Create($"{parent.FirstName} {parent.LastName}", parent.EmailAddress);

                        if (recipient.IsSuccess)
                            recipients.Add(recipient.Value);
                    }
                }
            }
        }

        recipients = recipients.DistinctBy(entry => entry.Email).ToList();

        return recipients;
    }
}

namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForParentPortal;

using Abstractions.Messaging;
using Application.Interfaces.Services;
using AppSettings.Models;
using Core.Enums;
using Core.Models.AppSettings.Enums;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactListForParentPortalQueryHandler 
    : IQueryHandler<GetContactListForParentPortalQuery, List<StudentSupportContactResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger _logger;

    public GetContactListForParentPortalQueryHandler(
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IAppSettingsService appSettings,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _appSettings = appSettings;
        _logger = logger.ForContext<GetContactListForParentPortalQuery>();
    }

    public async Task<Result<List<StudentSupportContactResponse>>> Handle(GetContactListForParentPortalQuery request, CancellationToken cancellationToken)
    {
        List<StudentSupportContactResponse> response = new();

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find Student with Id {id}", request.StudentId);

            return Result.Failure<List<StudentSupportContactResponse>>(StudentErrors.NotFound(request.StudentId));
        }

        // Add Counsellor
        ContactsConfiguration? counsellorConfiguration = await _appSettings.Contacts(ContactPosition.Counsellor, cancellationToken);

        if (counsellorConfiguration is null)
        {
            _logger.Warning("Could not load configuration data for Contacts");

            return response;
        }

        foreach (var staffMember in counsellorConfiguration.Contacts)
        {
            if (!staffMember.Value.Contains(student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram))
                continue;

            response.Add(new(
                staffMember.Key.Name.FirstName,
                staffMember.Key.Name.LastName,
                staffMember.Key.Name.DisplayName,
                staffMember.Key.EmailAddress.Email,
                string.Empty,
                "Support",
                "School Counsellor"));
        }

        // Add Careers Advisor
        ContactsConfiguration? careersAdvisorConfiguration = await _appSettings.Contacts(ContactPosition.CareersAdvisor, cancellationToken);

        if (careersAdvisorConfiguration is null)
        {
            _logger.Warning("Could not load configuration data for Contacts");

            return response;
        }

        foreach (var staffMember in careersAdvisorConfiguration.Contacts)
        {
            if (!staffMember.Value.Contains(student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram))
                continue;

            response.Add(new(
                staffMember.Key.Name.FirstName,
                staffMember.Key.Name.LastName,
                staffMember.Key.Name.DisplayName,
                staffMember.Key.EmailAddress.Email,
                string.Empty,
                "Support",
                "Careers Advisor"));
        }

        // Add Librarian
        ContactsConfiguration? librarianConfiguration = await _appSettings.Contacts(ContactPosition.Librarian, cancellationToken);

        if (librarianConfiguration is null)
        {
            _logger.Warning("Could not load configuration data for Contacts");

            return response;
        }

        foreach (var staffMember in librarianConfiguration.Contacts)
        {
            if (!staffMember.Value.Contains(student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram))
                continue;
            
            response.Add(new(
                staffMember.Key.Name.FirstName,
                staffMember.Key.Name.LastName,
                staffMember.Key.Name.DisplayName,
                staffMember.Key.EmailAddress.Email,
                string.Empty,
                "Support",
                "School Librarian"));
        }

        // Add Admin office and Tech Support
        response.Add(StudentSupportContactResponse.GetDefault);
        response.Add(StudentSupportContactResponse.GetSupport);

        // Add class teachers
        List<Offering> offerings = await _offeringRepository.GetByStudentId(request.StudentId, cancellationToken);

        foreach (Offering offering in offerings)
        {
            List<StaffMember> members = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

            if (members.Count == 0)
            {
                _logger.Warning("Could not find any teacher for Class {name} with Id {id}", offering.Name, offering.Id);

                continue;
            }

            Course? course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            foreach (StaffMember member in members)
            {
                response.Add(new(
                    member.Name.FirstName,
                    member.Name.LastName,
                    member.Name.DisplayName,
                    member.EmailAddress.Email,
                    string.Empty,
                    "Teacher",
                    course is not null ? $"{offering.Name} - {course.Name}" : offering.Name));
            }
        }

        ContactsConfiguration? lastConfiguration = await _appSettings.Contacts(ContactPosition.LearningSupport, cancellationToken);

        if (lastConfiguration is null)
        {
            _logger.Warning("Could not load configuration data for Contacts");

            return response;
        }

        foreach (var staffMember in librarianConfiguration.Contacts)
        {
            if (!staffMember.Value.Contains(student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram))
                continue;

            response.Add(new(
                staffMember.Key.Name.FirstName,
                staffMember.Key.Name.LastName,
                staffMember.Key.Name.DisplayName,
                staffMember.Key.EmailAddress.Email,
                string.Empty,
                "Support",
                "Learning Support"));
        }

        return response;
    }
}

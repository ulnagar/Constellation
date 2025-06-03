namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForParentPortal;

using Abstractions.Messaging;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
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
    private readonly ILogger _logger;
    private readonly AppConfiguration _configuration;

    public GetContactListForParentPortalQueryHandler(
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetContactListForParentPortalQuery>();
        _configuration = configuration.Value;
    }

    public async Task<Result<List<StudentSupportContactResponse>>> Handle(GetContactListForParentPortalQuery request, CancellationToken cancellationToken)
    {
        List<StudentSupportContactResponse> response = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not find Student with Id {id}", request.StudentId);

            return Result.Failure<List<StudentSupportContactResponse>>(StudentErrors.NotFound(request.StudentId));
        }

        if (_configuration.Contacts is null)
        {
            _logger.Warning("Could not load configuration data for Contacts");

            return response;
        }

        // Add Counsellor
        foreach (StaffId staffId in _configuration.Contacts.CounsellorIds)
        {
            StaffMember member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", staffId);

                continue;
            }

            response.Add(new(
                member.Name.FirstName,
                member.Name.LastName,
                member.Name.DisplayName,
                member.EmailAddress.Email,
                string.Empty,
                "Support",
                "School Counsellor"));
        }

        // Add Careers Advisor
        foreach (StaffId staffId in _configuration.Contacts.CareersAdvisorIds)
        {
            StaffMember member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", staffId);

                continue;
            }

            response.Add(new(
                member.Name.FirstName,
                member.Name.LastName,
                member.Name.DisplayName,
                member.EmailAddress.Email,
                string.Empty,
                "Support",
                "Careers Advisor"));
        }

        // Add Librarian
        foreach (StaffId staffId in _configuration.Contacts.LibrarianIds)
        {
            StaffMember member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", staffId);

                continue;
            }

            response.Add(new(
                member.Name.FirstName,
                member.Name.LastName,
                member.Name.DisplayName,
                member.EmailAddress.Email,
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

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
            return response;

        bool success = _configuration.Contacts.LearningSupportIds.TryGetValue(enrolment.Grade, out List<StaffId> lastStaffIds);

        if (!success)
            return response;
        
        foreach (StaffId staffId in lastStaffIds)
        {
            StaffMember member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is not null)
            {
                response.Add(new(
                    member.Name.FirstName,
                    member.Name.LastName,
                    member.Name.DisplayName,
                    member.EmailAddress.Email,
                    string.Empty,
                    "Support",
                    "Learning Support"));
            }
        }

        return response;
    }
}

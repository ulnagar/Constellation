namespace Constellation.Application.Contacts.GetContactListForParentPortal;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
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
        foreach (string staffId in _configuration.Contacts.CounsellorIds)
        {
            Staff member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", staffId);

                continue;
            }

            response.Add(new(
                member.FirstName,
                member.LastName,
                member.DisplayName,
                member.EmailAddress,
                string.Empty,
                "Support",
                "School Counsellor"));
        }

        // Add Careers Advisor
        foreach (string staffId in _configuration.Contacts.CareersAdvisorIds)
        {
            Staff member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger.Warning("Could not find Staff with Id {id}", staffId);

                continue;
            }

            response.Add(new(
                member.FirstName,
                member.LastName,
                member.DisplayName,
                member.EmailAddress,
                string.Empty,
                "Support",
                "Careers Advisor"));
        }

        // Add Admin office and Tech Support
        response.Add(StudentSupportContactResponse.GetDefault);
        response.Add(StudentSupportContactResponse.GetSupport);

        // Add class teachers
        List<Offering> offerings = await _offeringRepository.GetByStudentId(request.StudentId, cancellationToken);

        foreach (Offering offering in offerings)
        {
            List<Staff> members = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

            if (members.Count == 0)
            {
                _logger.Warning("Could not find any teacher for Class {name} with Id {id}", offering.Name, offering.Id);

                continue;
            }

            Course? course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            foreach (Staff member in members)
            {
                response.Add(new(
                    member.FirstName,
                    member.LastName,
                    member.DisplayName,
                    member.EmailAddress,
                    string.Empty,
                    "Teacher",
                    course is not null ? $"{offering.Name} - {course.Name}" : offering.Name));
            }
        }

        bool success = _configuration.Contacts.LearningSupportIds.TryGetValue(student.CurrentGrade, out List<string> lastStaffIds);

        if (success)
        {
            foreach (string staffId in lastStaffIds)
            {
                Staff member = await _staffRepository.GetById(staffId, cancellationToken);

                if (member is not null)
                {
                    response.Add(new(
                        member.FirstName,
                        member.LastName,
                        member.DisplayName,
                        member.EmailAddress,
                        string.Empty,
                        "Support",
                        "Learning Support"));
                }
            }
        }

        return response;
    }
}
